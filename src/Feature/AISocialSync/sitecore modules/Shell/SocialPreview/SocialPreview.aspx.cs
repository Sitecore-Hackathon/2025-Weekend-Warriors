using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Layouts;
using System;
using System.Threading.Tasks;
using HttpMethod = System.Net.Http.HttpMethod;
using System.Security.Cryptography;
using System.Net.Http;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using Sitecore.Diagnostics;

namespace AISocialSync.sitecore_modules.Shell.SocialPreview
{
    public partial class SocialPreview : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblTwitterResponse.Visible = false;
                var itemId = Request.QueryString["itemId"];
                if (!string.IsNullOrEmpty(itemId))
                {
                    // Get the current item
                    var database = Sitecore.Configuration.Factory.GetDatabase("master");
                    Item currentItem = database.GetItem(new ID(itemId));

                    // Set the preview details
                    if (currentItem != null && IfItemHasPresentation(currentItem))
                    {
                        lblOriginalTextCms.Text = currentItem.Fields[Constants.TitleFieldID]?.Value;


                        if (currentItem.Fields[Constants.SelectTopicFieldID] != null)
                        {
                            Sitecore.Data.Fields.MultilistField multiselectField = currentItem.Fields[Constants.SelectTopicFieldID];
                            Sitecore.Data.Items.Item[] items = multiselectField.GetItems();

                            if (items != null && items.Length > 0)
                            {
                                lblTrendsFromCMS.Text = string.Join(", ", items.Select(x => x.Fields[Constants.TrendTitleFieldID]?.Value));
                                lblOriginalTextCms.Text = null;
                            }
                        }
                    }
                    else
                    {
                        lblOriginalTextCms.Text = "current item is null";
                    }
                }
            }
        }

        private bool IfItemHasPresentation(Item item)
        {
            // Get the layout definition of the context item
            LayoutDefinition layoutDefinition = LayoutDefinition.Parse(item.Fields[Sitecore.FieldIDs.LayoutField].Value);
            // Ensure the layout definition exists
            if (layoutDefinition != null)
            {
                return true;
            }
            return false;
        }
        protected void PostToTwitter_Click(object sender, EventArgs e)
        {
            // Twitter API integration
            var consumerKey = Sitecore.Configuration.Settings.GetSetting("Twitter.ConsumerKey");
            var consumerSecret = Sitecore.Configuration.Settings.GetSetting("Twitter.ConsumerSecret");
            var accessToken = Sitecore.Configuration.Settings.GetSetting("Twitter.AccessToken"); ;
            var accessTokenSecret = Sitecore.Configuration.Settings.GetSetting("Twitter.AccessTokenSecret");
            var publishedTweet = PostTweet(lblOriginalTextCms.Text, consumerKey, consumerSecret, accessToken, accessTokenSecret);
        }

        protected void TrendingPostToTwitter_Click(object sender, EventArgs e)
        {
            // Twitter API integration
            var consumerKey = Sitecore.Configuration.Settings.GetSetting("Twitter.ConsumerKey");
            var consumerSecret = Sitecore.Configuration.Settings.GetSetting("Twitter.ConsumerSecret");
            var accessToken = Sitecore.Configuration.Settings.GetSetting("Twitter.AccessToken"); ;
            var accessTokenSecret = Sitecore.Configuration.Settings.GetSetting("Twitter.AccessTokenSecret");
            var publishedTweet = PostTweet(lblRewriteWithTrendingAI.Text, consumerKey, consumerSecret, accessToken, accessTokenSecret);
        }
        #region Twitter
        private async Task PostTweet(string text, string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            try
            {
                var apiUrl = Sitecore.Configuration.Settings.GetSetting("Twitter.ApiUrl");
                var timstamp = CreateTimestamp();
                var nonce = CreateNonce();
                var body = System.Text.Json.JsonSerializer.Serialize(new { text });
                var uri = new Uri(apiUrl);

                var request = new HttpRequestMessage
                {
                    RequestUri = uri,
                    Method = HttpMethod.Post,
                    Content = new StringContent(body, Encoding.ASCII, "application/json")
                };

                var signatureBase64 = CreateSignature(uri.ToString(), "POST", nonce, timstamp, consumerKey, accessToken,
                 consumerSecret, accessTokenSecret);

                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth",
                        $@"oauth_consumer_key=""{Uri.EscapeDataString(consumerKey)}""" +
                        $@",oauth_token=""{Uri.EscapeDataString(accessToken)}""" +
                        $@",oauth_signature_method=""HMAC-SHA1"",oauth_timestamp=""{Uri.EscapeDataString(timstamp)}""" +
                        $@",oauth_nonce=""{Uri.EscapeDataString(nonce)}"",oauth_version=""1.0""" +
                        $@",oauth_signature=""{Uri.EscapeDataString(signatureBase64)}""");

                HttpClient httpClient = new HttpClient();
                var response = await httpClient.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    response.EnsureSuccessStatusCode();
                    btnPostToTwitter.Visible = false;
                    btnAIResponse.Visible = false;

                    btnAITrendResponse.Visible = false;
                    btnTrendingPostToTwitter.Visible = false;

                    lblTwitterResponse.Visible = true;
                    lblTwitterResponse.Text = "Posted Successfully";
                    lblTwitterResponse.ForeColor = System.Drawing.Color.ForestGreen;

                }
                else
                {
                    lblTwitterResponse.Visible = true;
                    lblTwitterResponse.Text = "Something went wrong";
                    lblTwitterResponse.ForeColor = System.Drawing.Color.IndianRed;
                }
            }
            catch (Exception ex)
            {
                lblTwitterResponse.Visible = true;
                lblTwitterResponse.Text = "Something went wrong";
                lblTwitterResponse.ForeColor = System.Drawing.Color.IndianRed;
                Sitecore.Diagnostics.Log.Error($"Error at AISocialSync.sitecore_modules.Shell.SocialPreview.PostTweet - {ex.StackTrace}", this);
            }
        }

        private string CreateSignature(string url, string method, string nonce, string timestamp, string consumerKey, string accessToken,
          string consumerSecret, string accessTokenSecret)
        {
            var parameters = new Dictionary<string, string>();

            parameters.Add("oauth_consumer_key", consumerKey);
            parameters.Add("oauth_nonce", nonce);
            parameters.Add("oauth_signature_method", "HMAC-SHA1");
            parameters.Add("oauth_timestamp", timestamp);
            parameters.Add("oauth_token", accessToken);
            parameters.Add("oauth_version", "1.0");

            var sigBaseString = CombineQueryParams(parameters);

            var signatureBaseString =
                method.ToString() + "&" +
                Uri.EscapeDataString(url) + "&" +
                Uri.EscapeDataString(sigBaseString.ToString());

            var compositeKey =
                Uri.EscapeDataString(consumerSecret) + "&" +
                Uri.EscapeDataString(accessTokenSecret);

            using (var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(compositeKey)))
            {
                return Convert.ToBase64String(hasher.ComputeHash(
                    Encoding.ASCII.GetBytes(signatureBaseString)));
            }
        }
        private string CreateTimestamp()
        {
            var totalSeconds = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc))
                .TotalSeconds;

            return Convert.ToInt64(totalSeconds).ToString();
        }

        private string CreateNonce()
        {
            return Convert.ToBase64String(
                new ASCIIEncoding().GetBytes(
                    DateTime.Now.Ticks.ToString()));
        }
        private string CombineQueryParams(Dictionary<string, string> parameters)
        {
            var sb = new StringBuilder();

            var first = true;

            foreach (var param in parameters)
            {
                if (!first)
                {
                    sb.Append("&");
                }

                sb.Append(param.Key);
                sb.Append("=");
                sb.Append(Uri.EscapeDataString(param.Value));

                first = false;
            }

            return sb.ToString();
        }
        #endregion

        protected async void btnAIResponse_Click(object sender, EventArgs e)
        {
            // Call the OpenAI API for each trending topic
            string aiContent = await GetContentFromOpenAi(string.Empty);
            lblOriginalTextCms.Text = aiContent;
        }

        //private async Task<string> GetRegeneratedTextFromOpenAi()
        //{
        //    try
        //    {
        //        var openAIApiKey = Sitecore.Configuration.Settings.GetSetting("OpenAI.ApiKey");
        //        var openAIApiUrl = Sitecore.Configuration.Settings.GetSetting("OpenAI.ApiUrl");
        //        using (var client = new HttpClient())
        //        {
        //            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAIApiKey}");
        //            var requestBody = new
        //            {
        //                model = "gpt-4o-mini",
        //                messages = new[]
        //            {
        //            new { role = "system", content = "You are a social media content assistant." },
        //            new { role = "user", content = $"Please regenerate the following tweet to make it more engaging: '{lblOriginalTextCms.Text}'" }
        //        },
        //                max_tokens = 280,
        //                temperature = 0.7
        //            };

        //            var jsonRequest = JsonConvert.SerializeObject(requestBody);
        //            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        //            var response = await client.PostAsync(openAIApiUrl, content);
        //            response.EnsureSuccessStatusCode();

        //            var responseJson = await response.Content.ReadAsStringAsync();
        //            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseJson);
        //            var generatedTweet = jsonResponse.choices[0].message.content.ToString().Trim();

        //            return generatedTweet;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        lblTwitterResponse.Visible = true;
        //        lblTwitterResponse.Text = "Something went wrong" + ex.StackTrace;
        //    }
        //    return null;
        //}


        protected async void btnAITrendResponse_Click(object sender, EventArgs e)
        {
            var trendingTopic = lblTrendsFromCMS.Text;
            // Call the OpenAI API for each trending topic
            string aiContent = await GetContentFromOpenAi(trendingTopic);
            lblRewriteWithTrendingAI.Text = aiContent;
        }

        private async Task<string> GetContentFromOpenAi(string topic)
        {
            try
            {
                var openAIApiKey = Sitecore.Configuration.Settings.GetSetting("OpenAI.ApiKey");
                var openAIApiUrl = Sitecore.Configuration.Settings.GetSetting("OpenAI.ApiUrl");
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAIApiKey}");
                    string prompt = string.Empty;
                    if (!string.IsNullOrEmpty(topic))
                    {
                        prompt = $"Please generate an engaging social media post for the following trending topics:\n{topic}";
                    }
                    else
                    {
                        prompt = $"Please regenerate the following tweet to make it more engaging: '{lblOriginalTextCms.Text}'";
                    }
                    

                    var requestBody = new
                    {
                        model = "gpt-4o-mini",
                        messages = new[]
                    {
                    new { role = "system", content = "You are a social media content assistant." },
                    new { role = "user", content = prompt }
                },
                        max_tokens = 280, // Allow longer responses for multiple trends
                        temperature = 0.7
                    };

                    var jsonRequest = JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(openAIApiUrl, content);
                    response.EnsureSuccessStatusCode();

                    var responseJson = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseJson);
                    var generatedText = jsonResponse.choices[0].message.content.ToString().Trim();

                    return generatedText;
                }
            }
            catch (Exception ex)
            {
                lblTwitterResponse.Visible = true;
                lblTwitterResponse.Text = "Something went wrong";
                lblTwitterResponse.ForeColor = System.Drawing.Color.IndianRed;
                Sitecore.Diagnostics.Log.Error($"Error at AISocialSync.sitecore_modules.Shell.SocialPreview.GetContentFromOpenAi - {ex.StackTrace}", this);
            }
            return null;
        }
    }
}