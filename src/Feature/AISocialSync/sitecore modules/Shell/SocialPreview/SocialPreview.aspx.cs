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
                        lblOriginalTextCms.Text = currentItem["{A60ACD61-A6DB-4182-8329-C957982CEC74}"];

                        if (currentItem.Fields["SelectTopic"] != null)
                        {
                            Sitecore.Data.Fields.MultilistField multiselectField = currentItem.Fields["SelectTopic"];
                            Sitecore.Data.Items.Item[] items = multiselectField.GetItems();

                            if (items != null && items.Length > 0)
                            {
                                lblTrendsFromCMS.Text = string.Join(",", items.Select(x => x.Fields["TrendTitle"]?.Value));
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
    }
}