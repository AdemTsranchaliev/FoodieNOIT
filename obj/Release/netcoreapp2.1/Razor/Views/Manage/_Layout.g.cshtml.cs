#pragma checksum "C:\Users\Asus\source\repos\Foodie1\Foodie1\Views\Manage\_Layout.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "0362a0f063ee223f0412d842137fa1926c600983"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Manage__Layout), @"mvc.1.0.view", @"/Views/Manage/_Layout.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Manage/_Layout.cshtml", typeof(AspNetCore.Views_Manage__Layout))]
namespace AspNetCore
{
    #line hidden
#line 11 "C:\Users\Asus\source\repos\Foodie1\Foodie1\Views\_ViewImports.cshtml"
using System;

#line default
#line hidden
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#line 1 "C:\Users\Asus\source\repos\Foodie1\Foodie1\Views\_ViewImports.cshtml"
using Microsoft.AspNetCore.Identity;

#line default
#line hidden
#line 2 "C:\Users\Asus\source\repos\Foodie1\Foodie1\Views\_ViewImports.cshtml"
using Foodie1;

#line default
#line hidden
#line 3 "C:\Users\Asus\source\repos\Foodie1\Foodie1\Views\_ViewImports.cshtml"
using Foodie1.Models;

#line default
#line hidden
#line 4 "C:\Users\Asus\source\repos\Foodie1\Foodie1\Views\_ViewImports.cshtml"
using Foodie1.Models.AccountViewModels;

#line default
#line hidden
#line 5 "C:\Users\Asus\source\repos\Foodie1\Foodie1\Views\_ViewImports.cshtml"
using Foodie1.Models.ManageViewModels;

#line default
#line hidden
#line 6 "C:\Users\Asus\source\repos\Foodie1\Foodie1\Views\_ViewImports.cshtml"
using Foodie1.Models.RestaurantViewModels;

#line default
#line hidden
#line 8 "C:\Users\Asus\source\repos\Foodie1\Foodie1\Views\_ViewImports.cshtml"
using Foodie1.Controllers;

#line default
#line hidden
#line 9 "C:\Users\Asus\source\repos\Foodie1\Foodie1\Views\_ViewImports.cshtml"
using Foodie1.Data;

#line default
#line hidden
#line 10 "C:\Users\Asus\source\repos\Foodie1\Foodie1\Views\_ViewImports.cshtml"
using System.Globalization;

#line default
#line hidden
#line 1 "C:\Users\Asus\source\repos\Foodie1\Foodie1\Views\Manage\_ViewImports.cshtml"
using Foodie1.Views.Manage;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"0362a0f063ee223f0412d842137fa1926c600983", @"/Views/Manage/_Layout.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"3d4c33097155458e56da0e7bf9f16cf61390e617", @"/Views/_ViewImports.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"c0215451211b9b120e3e9605d7c97d8e86a6152b", @"/Views/Manage/_ViewImports.cshtml")]
    public class Views_Manage__Layout : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 1 "C:\Users\Asus\source\repos\Foodie1\Foodie1\Views\Manage\_Layout.cshtml"
   
    Layout = "/Views/Shared/_Layout.cshtml";

#line default
#line hidden
            BeginContext(54, 163, true);
            WriteLiteral("\r\n<h2>Manage your account</h2>\r\n\r\n<div>\r\n    <h4>Change your account settings</h4>\r\n    <hr />\r\n    <div class=\"row\">\r\n        <div class=\"col-md-3\">\r\n            ");
            EndContext();
            BeginContext(218, 37, false);
#line 12 "C:\Users\Asus\source\repos\Foodie1\Foodie1\Views\Manage\_Layout.cshtml"
       Write(await Html.PartialAsync("_ManageNav"));

#line default
#line hidden
            EndContext();
            BeginContext(255, 62, true);
            WriteLiteral("\r\n        </div>\r\n        <div class=\"col-md-9\">\r\n            ");
            EndContext();
            BeginContext(318, 12, false);
#line 15 "C:\Users\Asus\source\repos\Foodie1\Foodie1\Views\Manage\_Layout.cshtml"
       Write(RenderBody());

#line default
#line hidden
            EndContext();
            BeginContext(330, 40, true);
            WriteLiteral("\r\n        </div>\r\n    </div>\r\n</div>\r\n\r\n");
            EndContext();
            DefineSection("Scripts", async() => {
                BeginContext(388, 6, true);
                WriteLiteral("\r\n    ");
                EndContext();
                BeginContext(395, 41, false);
#line 21 "C:\Users\Asus\source\repos\Foodie1\Foodie1\Views\Manage\_Layout.cshtml"
Write(RenderSection("Scripts", required: false));

#line default
#line hidden
                EndContext();
                BeginContext(436, 2, true);
                WriteLiteral("\r\n");
                EndContext();
            }
            );
            BeginContext(441, 10, true);
            WriteLiteral("        \r\n");
            EndContext();
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; }
    }
}
#pragma warning restore 1591