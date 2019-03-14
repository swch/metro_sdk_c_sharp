using System;
using Xunit;

using Switch.Security.Cryptography;


namespace cardsavr_tests
{
    public class EcdhTests
    {
        // the key-pair and public-key of our Ecdh class. 
        // these are used for computing a shared secret.
        private readonly string KEY_PAIR = "TwIAADCCAksCAQAwgewGByqGSM49AgEwgeACAQEwLAYHKoZIzj0BAQIhAP////8AAAABAAAAAAAAAAAAAAAA////////////////MEQEIP////8AAAABAAAAAAAAAAAAAAAA///////////////8BCBaxjXYqjqT57PrvVV2mIa8ZR0GsMxTsPY7zjw+J9JgSwRBBGsX0fLhLEJH+Lzm5WOkQPJ3A32BLeszoPShOUXYmMKWT+NC4v4af5uO5+tKfA+eFivOM1drMV7Oy7ZAaDe/UfUCIQD/////AAAAAP//////////vOb6racXnoTzucrC/GMlUQIBAQSCAVUwggFRAgEBBCAAiH+n1w55DwHf1KI1ImPg5hHoGDL39f9Gn8UXdxw/HqCB4zCB4AIBATAsBgcqhkjOPQEBAiEA/////wAAAAEAAAAAAAAAAAAAAAD///////////////8wRAQg/////wAAAAEAAAAAAAAAAAAAAAD///////////////wEIFrGNdiqOpPns+u9VXaYhrxlHQawzFOw9jvOPD4n0mBLBEEEaxfR8uEsQkf4vOblY6RA8ncDfYEt6zOg9KE5RdiYwpZP40Li/hp/m47n60p8D54WK84zV2sxXs7LtkBoN79R9QIhAP////8AAAAA//////////+85vqtpxeehPO5ysL8YyVRAgEBoUQDQgAEK8ccpS3LY45FkOl8MPR+bbNm4/NDN+gjSA5SdWcuKqfRq/hnxUYm4eiqS1POXoRzPxaI7r6a8KPjg6NTbF3DFjCCATMwgewGByqGSM49AgEwgeACAQEwLAYHKoZIzj0BAQIhAP////8AAAABAAAAAAAAAAAAAAAA////////////////MEQEIP////8AAAABAAAAAAAAAAAAAAAA///////////////8BCBaxjXYqjqT57PrvVV2mIa8ZR0GsMxTsPY7zjw+J9JgSwRBBGsX0fLhLEJH+Lzm5WOkQPJ3A32BLeszoPShOUXYmMKWT+NC4v4af5uO5+tKfA+eFivOM1drMV7Oy7ZAaDe/UfUCIQD/////AAAAAP//////////vOb6racXnoTzucrC/GMlUQIBAQNCAAQrxxylLctjjkWQ6Xww9H5ts2bj80M36CNIDlJ1Zy4qp9Gr+GfFRibh6KpLU85ehHM/Fojuvprwo+ODo1NsXcMW";
        private readonly string PUB_KEY = "BCvHHKUty2OORZDpfDD0fm2zZuPzQzfoI0gOUnVnLiqn0av4Z8VGJuHoqktTzl6Ecz8WiO6+mvCj44OjU2xdwxY=";

        // taken from the JS code: the public key of different key-pairs on the same curve.
        private readonly string[] REMOTE_PUB_KEYS = new string[] {
            "BDsTBR7LwWwYPVjkeTddCmDCVbeCtZqgiNvEQnnB4Nrrti4KvlBNzAMtZyxsqH58k9pRzaXxklV25pfI1A2aoeU=",
            "BP0/5A6u4qsZxb8ZPO+/j2ojJc9v28oZ4A594kW7ZVrLPZ1bZc73d0ViMQmV68UqCd5cbB8Kc+GRE4yHRl4snRo=",
            "BLApe7axIYP6K29Mwf0An4r/GkOE3PaqNWxIVNVguC283VXOIw+SHLtvbfivIEAuGmwgIaA41idL4trEZvoyndo=",
            "BB6e1vHbVbXyOT7V3kChwc35Rrm9Rz5tC/UXi8SLAeuZnPn4ge/PMX8MgyCVUalPD4OGsDont7r6TdRzjjjwGuY="
        };

        // also taken from the JS code: the shared secret computed by using our public key (above).
        // this value should be equal to the shared secret that we calculate.
        private readonly string[] SHARED_SECRETS = new string[] {
            "RxQMfnqs0cKSu+nkbazROunXKE7YMcpqUZ/soJbJmwM=",
            "zEY70XnXMtnRMqoDAoXK3ILqcqMcoGj9xZNVKr22af0=",
            "QsoBbjzycrLAOdvOFkeTx0OnffFiTu3h8VEl35U59cM=",
            "nDWVbXs5ZraATuaU8U62cEe4/8Hf7m0rkJCY03VVPuA="
        };

        public EcdhTests()
        {
        }

        [Fact]
        public void SaveAndRestoreForTests()
        {
            // the save/restore functionality is used during unit tests.
            EcdhBC ecdh1 = new EcdhBC();
            EcdhBC ecdh2 = new EcdhBC(ecdh1.GetBlob());
            Assert.Equal(ecdh1.PublicKey, ecdh2.PublicKey);
        }

        [Fact]
        public void SharedSecretBC()
        {
            EcdhBC ecdh = new EcdhBC(Convert.FromBase64String(KEY_PAIR));
            Assert.Equal(ecdh.PublicKey, PUB_KEY);

            for (int n = 0; n < REMOTE_PUB_KEYS.Length; ++n)
            {
                ecdh.ComputeSharedSecret(REMOTE_PUB_KEYS[n]);
                Assert.Equal(ecdh.SharedSecret, SHARED_SECRETS[n]);
            }
        }
    }
}
