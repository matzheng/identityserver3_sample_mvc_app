﻿using IdentityServer3.Core;
using IdentityServer3.Core.Services.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace sample_webapp
{
    public static class Users
    {
        public static List<InMemoryUser> Get()
        {
            return new List<InMemoryUser>
            {
                new InMemoryUser
                {
                    Username = "bob",
                    Password = "secret",
                    Subject = "1",
                    Claims = new []
                    {
                        new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                        new Claim(Constants.ClaimTypes.GivenName, "Bob"),
                        new Claim(Constants.ClaimTypes.Role, "Geek"),
                        new Claim(Constants.ClaimTypes.Role, "Foo"),
                        new Claim(Constants.ClaimTypes.WebSite, "some data")
                    }
                },
                new InMemoryUser
                {
                    Username = "tom",
                    Password = "secret",
                    Subject = "2",
                    Claims = new []
                    {
                        new Claim(Constants.ClaimTypes.FamilyName, "Don"),
                        new Claim(Constants.ClaimTypes.GivenName, "Tom"),
                        new Claim(Constants.ClaimTypes.Role, "Operator"),
                        new Claim(Constants.ClaimTypes.Role, "Supervisor")
                    }
                }
            };
        }
    }
}