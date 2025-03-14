﻿<%@ Page Async="true" Language="C#" AutoEventWireup="true" CodeBehind="SocialPreview.aspx.cs" Inherits="AISocialSync.sitecore_modules.Shell.SocialPreview.SocialPreview" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">

    <title>Social AI</title>
    <!-- Link Tailwind CSS (CDN) -->
    <link href="https://cdn.jsdelivr.net/npm/tailwindcss@2.2.19/dist/tailwind.min.css" rel="stylesheet" />
</head>
<body class="bg-gray-100">
   
    <form id="form1" runat="server">
       <h2>AI Social Content Generator</h2>
        <% if (!string.IsNullOrEmpty(lblOriginalTextCms.Text))
          { %>
        <div class="mx-auto mt-10">
            <div class="px-6 py-4">
                <h2 class="font-bold text-xl mb-2">Post description</h2>
                <p class="text-gray-700 text-base">
                    <asp:Label ID="lblOriginalTextCms" runat="server" CssClass="block text-lg font-medium text-gray-700"></asp:Label>
                </p>
            </div>

            <div class="px-6 py-4 flex justify-between items-center">
                <asp:Button CssClass="btnRewrite bg-blue-500 text-white py-2 px-4 rounded-md hover:bg-green-600" ID="btnAIResponse" runat="server" Text="Rewrite with AI!" OnClick="btnAIResponse_Click" />
                <asp:Button ID="btnPostToTwitter" runat="server" Text="Post to Twitter" CssClass="bg-blue-400 text-white py-2 px-4 rounded-md hover:bg-blue-500" OnClick="PostToTwitter_Click" />
            </div>

       
        </div>
        <% } %>
        

        <% if (!string.IsNullOrEmpty(lblTrendsFromCMS.Text))
          { %>        
        <div class="mx-auto mt-10 p-4">
            <div class="px-6 py-4">
                <h2 class="font-bold text-xl mb-2">Post description based on Trends</h2>
                <asp:Label ID="lblTrendsFromCMS" runat="server"></asp:Label>
                <p class="text-gray-700 text-base">
                    <asp:Label ID="lblRewriteWithTrendingAI" runat="server" CssClass="block text-sm font-medium text-gray-500"></asp:Label>
                </p>
            </div>

            <!-- Card Footer -->
            <div class="px-6 py-4 flex justify-between items-center">
                <asp:Button CssClass="btnRewrite bg-green-500 text-white py-2 px-4 rounded-md hover:bg-green-600" ID="btnAITrendResponse" runat="server" Text="Generate with Trend AI!" OnClick="btnAITrendResponse_Click" />
                <asp:Button ID="btnTrendingPostToTwitter" runat="server" Text="Post to Twitter" CssClass="bg-blue-400 text-white py-2 px-4 rounded-md hover:bg-blue-500" OnClick="TrendingPostToTwitter_Click" />
            </div>
        </div>
        <% } %>
         <div class="mx-auto mt-10 p-4">
             <asp:Label ID="lblTwitterResponse" runat="server" ForeColor="Red"></asp:Label>
         </div>
    </form>

</body>

</html>
