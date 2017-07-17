using System;
using System.Collections.Generic;
using System.Configuration;
using Amazon.Runtime;
using Amazon.CognitoIdentityProvider.Model;
using System.Globalization;


namespace Amaas.Core.Sdk.Authentication
{
    public class CognitoAuthentication
    {
        public string CheckPasswordAsync(string userName, string password)
        {
            string IdToken = "";
            try
            {
                var AWS_CLIENT_ID = ConfigurationManager.AppSettings["CLIENT_ID"];
                var AWS_REGION = Amazon.RegionEndpoint.USWest2; //TODO: change to the region your pool is in! i.e.:  Amazon.RegionEndpoint.USWest2
                String POOL_NAME = ConfigurationManager.AppSettings["POOL_NAME"];
                AnonymousAWSCredentials cred = new AnonymousAWSCredentials();

                // Identify your Cognito UserPool Provider
                using (var provider = new Amazon.CognitoIdentityProvider.AmazonCognitoIdentityProviderClient(cred, AWS_REGION))
                {
                    //Get the SRP variables A and a
                    var TupleAa = AuthenticationHelper.CreateAaTuple();

                    //Initiate auth with the generated SRP A
                    var authResponse = provider.InitiateAuth(new InitiateAuthRequest
                    {
                        ClientId = ConfigurationManager.AppSettings["CLIENT_ID"],
                        AuthFlow = Amazon.CognitoIdentityProvider.AuthFlowType.USER_SRP_AUTH,
                        AuthParameters = new Dictionary<string, string>()
                        {
                            { "USERNAME", userName },
                            { "SRP_A", TupleAa.Item1.ToString(16) }
                        }
                    });

                    //Now with the authResponse containing the password challenge for us, we need to 
                    //set up a reply
                    //ChallengeParameters SALT, SECRET_BLOCK, SRP_B, USERNAME, USER_ID_FOR_SRP

                    DateTime timestamp = TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
                    //The timestamp format returned to AWS _needs_ to be in US Culture
                    CultureInfo usCulture = new CultureInfo("en-US");
                    String timeStr = timestamp.ToString("ddd MMM d HH:mm:ss \"UTC\" yyyy", usCulture);

                    //Do the hard work to generate the claim we return to AWS
                    byte[] claim = AuthenticationHelper.authenticateUser(authResponse.ChallengeParameters["USERNAME"],
                                                    password,
                                                    POOL_NAME,
                                                    TupleAa,
                                                    authResponse.ChallengeParameters["SALT"],
                                                    authResponse.ChallengeParameters["SRP_B"],
                                                    authResponse.ChallengeParameters["SECRET_BLOCK"],
                                                    timeStr
                                                    );
                    String claimBase64 = System.Convert.ToBase64String(claim);

                    //Our response to AWS. If successful it will return an object with Tokens,
                    //if unsuccessful, it will throw an Exception that you should catch and handle.
                    var resp = provider.RespondToAuthChallenge(new RespondToAuthChallengeRequest
                    {
                        ChallengeName = authResponse.ChallengeName,
                        ClientId = AWS_CLIENT_ID,
                        ChallengeResponses = new Dictionary<string, string>()
                        {
                            { "PASSWORD_CLAIM_SECRET_BLOCK", authResponse.ChallengeParameters["SECRET_BLOCK"] },
                            { "PASSWORD_CLAIM_SIGNATURE", claimBase64 },
                            { "USERNAME", userName },
                            { "TIMESTAMP", timeStr }
                        }
                    });

                    IdToken = resp.AuthenticationResult.IdToken;
                }

                return IdToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return "false";
            }
        }
    }
}
