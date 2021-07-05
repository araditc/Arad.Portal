using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard
{
    public class SeedLinkModel
    {
        private void Method()
        {
            var menues = new List<MenuLinkModel>();
            menues.Add(
          new MenuLinkModel()
          {
              Icon = "fa fa-circle-o",
              Link = "#",
              IsActive = true,
              MenuTitle = "منوی اول از تست",
              MenuId = "28ed2ced-8433-4b1a-bee4-628f0985e381",
              Priority = 1,
              Children = new List<MenuLinkModel>() {
                    new MenuLinkModel(){
                    Icon = "fa fa-circle-o",
                    IsActive = true,
                    Link = "#",
                     MenuTitle = "زیر منوی اول از منوی تست یک",
                     MenuId = "5496fdea-42ae-4047-8057-5675ecae94f9",
                     Priority = 1,
                     Children = new List<MenuLinkModel>()
                     {
                       new MenuLinkModel(){
                        Icon = "fa fa-circle-o",
                        IsActive = true,
                        Link = "#",
                        MenuTitle = "1-1-1",
                        MenuId = "244d93bf-373a-4e7a-8cfa-684ed3d4f7ad",
                        Priority = 1,
                         Children = new List<MenuLinkModel>()
                         {
                             new MenuLinkModel(){
                                Icon = "fa fa-circle-o",
                                IsActive = true,
                                Link = "#",
                                MenuTitle = "1-1-1-1",
                                MenuId = "244d93bf-373a-4e7a-8cfa-684ed3d4f7ad",
                                Priority = 1,
                             Children = new List<MenuLinkModel>()

                         }
                         }
                     }
                     }
                }
           }
          });
            menues.Add(
          new MenuLinkModel()
          {
              Icon = "fa fa-circle-o",
              Link = "#",
              IsActive = true,
              MenuTitle = "منوی دوم از تست",
              MenuId = "2fc8f9bd-6de8-48b8-9c16-22eb8b4991f4",
              Priority = 1,
              Children = new List<MenuLinkModel>() {
                    new MenuLinkModel(){
                    Icon = "fa fa-circle-o",
                    IsActive = true,
                    Link = "#",
                     MenuTitle = "زیر منوی اول از منوی تست دو",
                     MenuId = "58d808b7-19e5-4ffe-a83b-725ee13b185c",
                     Priority = 1,
                     Children = new List<MenuLinkModel>()
                     {
                       new MenuLinkModel()
                       {
                        Icon = "fa fa-circle-o",
                        IsActive = true,
                        Link = "#",
                        MenuTitle = "2-1-1",
                        MenuId = "412a411c-20df-43ea-8245-e223b19ba2bf",
                        Priority = 1,
                         Children = new List<MenuLinkModel>()
                         {
                             new MenuLinkModel(){
                                Icon = "fa fa-circle-o",
                                IsActive = true,
                                Link = "#",
                                MenuTitle = "2-1-1-1",
                                MenuId = "05daedfc-9214-47a7-b1c9-59cc6e654e97",
                                Priority = 1,
                             Children = new List<MenuLinkModel>()
                         },
                             new MenuLinkModel(){
                                Icon = "fa fa-circle-o",
                                IsActive = true,
                                Link = "#",
                                MenuTitle = "2-1-1-2",
                                MenuId = "36aef629-0021-478b-8b42-d7da1658af3f",
                                Priority = 1,
                             Children = new List<MenuLinkModel>() }
                         }
                       },
                       new MenuLinkModel()
                       {
                        Icon = "fa fa-circle-o",
                        IsActive = true,
                        Link = "#",
                        MenuTitle = "2-1-2",
                        MenuId = "d629e12f-1ade-476d-a1c1-68b9677ef19d",
                        Priority = 1,
                         Children = new List<MenuLinkModel>()
                         {
                             new MenuLinkModel(){
                                Icon = "fa fa-circle-o",
                                IsActive = true,
                                Link = "#",
                                MenuTitle = "2-1-2-1",
                                MenuId = "aa36db4f-c041-47bb-b977-b277634dc5f3",
                                Priority = 1,
                             Children = new List<MenuLinkModel>()
                         },
                             new MenuLinkModel(){
                                Icon = "fa fa-circle-o",
                                IsActive = true,
                                Link = "#",
                                MenuTitle = "2-1-2-2",
                                MenuId = "5598364c-45c1-4fea-b672-856b13a30c88",
                                Priority = 1,
                             Children = new List<MenuLinkModel>() }
                         }
                       }
                     }
                }
           }
          });

        }
        
       
    }
}
