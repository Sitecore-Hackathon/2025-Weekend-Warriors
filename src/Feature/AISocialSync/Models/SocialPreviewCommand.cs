using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace AISocialSync.Models
{
    public class SocialPreviewCommand : Command
  {
    public override void Execute(CommandContext context)
    {
      // Get the current item ID
      var itemId = context.Items[0].ID.ToString();
      // Open the dialog/overlay with the item ID as a query string parameter
      Sitecore.Context.ClientPage.Start(this, "Run", new ClientPipelineArgs { Parameters = { ["itemId"] = itemId } });
    }

    protected void Run(ClientPipelineArgs args)
    {
      if (!args.IsPostBack)
      {
        var itemId = args.Parameters["itemId"];
        SheerResponse.ShowModalDialog($"/sitecore modules/Shell/SocialPreview/SocialPreview.aspx?itemId={itemId}", "800", "600", "AI Social Content Generator", true);
        args.WaitForPostBack();
      }
    }
  }
}