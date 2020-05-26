
# Switch CardSavr SDK for C#

## Setup

I used VS Code to install this package. There is a file called metro_sdk_c_sharp.code-workspace that contains the three "projects".  

* cardsavr - This is the core SDK that you will be programming against
* cardsavr_e2e - This contains some great examples of how to use the SDK
* cardsavr_tests - Simple tests that unit test the encryption and security libraries within the SDK

Cardsavr contains a set of REST endpoints.  Although there are APIs for over a dozen entities, the SDK is primarily designed to address accessing a subset specifically for creating cardholders, and creating the data structures necessar for posting a job.  Once a job is created, there needs to be frontend code that listens to feedback from the Virtual Browser System (VBS), and there is a javascript SDK along with code samples here:  https://github.com/swch/Strivve-SDK

Additionally, in order to use Cardsavr, you will require application keys, and a username/password for your agent.  This can be achieved by contacting developer-support@strivve.com.

## Users 

All cardholder activity requires an active cardholder.  Users can be created using randomly generated usernames or based on usernames in a different sysetm (SSO).  

## Addresses

All cardholders must have addresses.  A cardholder may have mutliple addressses to use with different cards.

## Cards

In order to place a job, a cardholder must have a valid card on file.

## Credential Grants

These are actually are a part of the Users endpoints, but are essentail for assuming a cardholder and posting this other information.  Grants can also be sent to other parts of the system (e.g. the Javascritp SDK within the frontend) so jobs can be interactively posted.  Cards and addresses can only be saved by a cardholder, so your code will have to assume the role of a cardholder using a grant.

## Accounts, Merchant Sites, Jobs

Although there are endpoints for these entities, these are generally the responsibility of the frontend to save.  There are examples on how to use them within the e2e folder.

## Running Cardsavr

To verify the tests function correctly, you may run:  

`dotnet test`

from the cardsavr_tests folder.

To verify that your agent and integrator keys are configured correctly (See cardsavr_e2e/Context.cs), run Program.cs

`dotnet run`

## Conclusion

That's it!  We may occasionally add/remove functionality to/from the SDK, so we recommend forking the repo and keeping an eye out for changes.  We haven't fully figured out how to handle versioning, so all changes will need to be backward compatible effective 6/1/2020.
