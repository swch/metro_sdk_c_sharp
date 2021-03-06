﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Net;

using Switch.CardSavr.Http;
using Switch.Security;
using Switch.CardSavr.Exceptions;
using Xunit;
using Xunit.Priority;

namespace cardsavr_e2e
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    [Collection("CardsavrSession collection")]
    public class JobTests
    {
        CardsavrSession session;

        public JobTests(CardsavrSession session)
        {
            this.session = session;
        }

        [Fact, Priority(1)]
        public async void TestCreateJob() {

            List<Cardholder> cardholders = new List<Cardholder>();

            // create cardholders.
            PropertyBag bag = new PropertyBag();
            int count = 2;

            string[] safeKeys = new string[count];

            for (int n = 0; n < count; ++n)
            {
                // generate using an easily reproducible safe-key.
                bag["cuid"] = $"{CardsavrSession.e2e_identifier}_job_tests_{CardsavrSession.random.Next(10000)}_{n}";
                bag["first_name"] = $"Otto_{n}_cardholder";
                bag["last_name"] = $"Matic_{n}_cardholder";
                bag["email"] = $"cardsavr_e2e_{CardsavrSession.random.Next(10000)}@gmail.com";

                safeKeys[n] = n % 2 == 0 ? null : CardsavrSession.GenerateBogus32BitPassword(bag.GetString("cuid"));
                CardSavrResponse<Cardholder> result = await this.session.http.CreateCardholderAsync(bag, safeKeys[n]);
                cardholders.Add(result.Body);
            }
            
            Assert.Equal(cardholders.Count, count);

            for (int n = 0; n < count; ++n) {

                bag.Clear();
                bag["cardholder_id"] = cardholders[n].id;
                bag["merchant_site_id"] = 1;
                bag["username"] = "good_email";
                bag["password"] = "";

                CardSavrResponse<Account> account = await this.session.http.CreateAccountAsync(bag, safeKeys[n]);
                Assert.Equal(HttpStatusCode.Created, account.StatusCode);

                bag.Clear();
                bag = new PropertyBag()
                {
                    { "cardholder_id", cardholders[n].id },
                    { "requesting_brand", "mbudos" },
                    { "status", "INITIATED" },
                    { "account_id", account.Body.id }
                };
                
                CardSavrResponse<SingleSiteJob> job = await this.session.http.CreateSingleSiteJobAsync(bag, safeKeys[n]);
                Assert.Equal(HttpStatusCode.Created, job.StatusCode);

                CardSavrResponse<List<SingleSiteJob>> singleJobs = await this.session.http.GetSingleSiteJobsAsync(
                    new NameValueCollection() {
                        { "ids", job.Body.id.ToString() }
                    }
                );
                log.Info($"retrieved {singleJobs.Body.Count} single-site jobs.");
                foreach (SingleSiteJob sj in singleJobs.Body)
                    Assert.Equal(job.Body.id, sj.id);
    
                bag.Clear();
                bag["status"] = "CANCEL_REQUESTED";
                
                // NOT BACKWARD COMPATIBLE - Only using agent now
                job = await this.session.http.UpdateSingleSiteJobAsync(job.Body.id, bag, null);
                log.Info($"{job.Body.id}: {job.Body.status}");
                Assert.Equal(job.Body.status, bag["status"]);

            }            
            
            CardSavrResponse<List<Cardholder>> cardholderResponse = await this.session.http.GetCardholdersAsync(null);

            foreach (Cardholder c in cardholderResponse.Body) {
                if (c.cuid.StartsWith($"{CardsavrSession.e2e_identifier}_job_tests")) {
                    await this.session.http.DeleteCardholderAsync(c.id);
                    count--;
                }
            }
            Assert.Equal(0, count);
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    }
}
