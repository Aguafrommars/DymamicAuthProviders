// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Aguacongas.AspNetCore.Authentication.TestBase
{
    /// <summary>
    /// Base test suite to verify if the store implementation work as expecter
    /// </summary>
    /// <typeparam name="TSchemeDefinition">The type of the scheme definition.</typeparam>
    public abstract class DynamicManagerTestBase<TSchemeDefinition>
        where TSchemeDefinition: ISchemeDefinition, new()
    {
        [SuppressMessage("Critical Code Smell", "S4487:Unread \"private\" fields should be removed", Justification = "XUnit log.")]
        private readonly ITestOutputHelper _output;
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicManagerTestBase{TSchemeDefinition}"/> class.
        /// </summary>
        /// <param name="output">The output.</param>
        protected DynamicManagerTestBase(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <summary>
        /// AddAsync should fail on suplicate scheme
        /// </summary>
        /// <returns></returns>
        [Fact]
        [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "Test fail on exception")]
        public async Task AddAsync_should_fail_on_duplicate_scheme()
        {
            var provider = CreateServiceProvider(options =>
            {
                options.AddCookie();
            });

            var cookieOptions = new CookieAuthenticationOptions
            {
                Cookie = new CookieBuilder
                {
                    Domain = "test"
                }
            };

            var scheme = Guid.NewGuid().ToString();
            var definition = new TSchemeDefinition
            {
                Scheme = scheme,
                DisplayName = "test",
                HandlerType = typeof(CookieAuthenticationHandler),
                Options = cookieOptions
            };

            var sut = provider.GetRequiredService<AuthenticationSchemeProviderWrapper>();

            await sut.AddAsync(definition);
            try
            {
                await sut.AddAsync(definition);
                throw new InvalidOperationException("AddAsync should fail on ducplicate scheme");
            }
#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable S108 // Nested blocks of code should not be left empty
            catch { }
#pragma warning restore S108 // Nested blocks of code should not be left empty
#pragma warning restore CA1031 // Do not catch general exception types
        }

        /// <summary>
        /// AddAsync method should add cookie handler.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddAsync_should_add_cookie_handler()
        {
            var eventCalled = false;
            Task onSignId(CookieSignedInContext context)
            {
                eventCalled = true;
                return Task.CompletedTask;
            }

            var provider = CreateServiceProvider(options =>
            {
                options.AddCookie(configure =>
                {
                    configure.Events.OnSignedIn = onSignId;
                });
            });

            var sut = provider.GetRequiredService<AuthenticationSchemeProviderWrapper>();
            Assert.Contains(typeof(CookieAuthenticationHandler), sut.GetManagedHandlerTypes());

            var cookieOptions = new CookieAuthenticationOptions
            {
                Cookie = new CookieBuilder
                {
                    Domain = "test"
                }
            };

            var scheme = Guid.NewGuid().ToString();
            var definition = new TSchemeDefinition
            {
                Scheme = scheme,
                DisplayName = "test",
                HandlerType = typeof(CookieAuthenticationHandler),
                Options = cookieOptions
            };
            await sut.AddAsync(definition);
            var state = await VerifyAddedAsync<CookieAuthenticationOptions>(scheme, provider);

            var httpContext = new Mock<HttpContext>().Object;
            state.options.Events.OnSignedIn(new CookieSignedInContext(
                httpContext,
                state.scheme as AuthenticationScheme,
                new ClaimsPrincipal(),
                new AuthenticationProperties(),
                state.options as CookieAuthenticationOptions));

            Assert.True(eventCalled);
        }

        /// <summary>
        /// AddAsync method should add facebook handler.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddAsync_should_add_facebook_handler()
        {
            var eventCalled = false;
            Task onCreatingTicket(OAuthCreatingTicketContext context)
            {
                eventCalled = true;
                return Task.CompletedTask;
            }

            var provider = CreateServiceProvider(options =>
            {
                options.AddFacebook(configure =>
                {
                    configure.Events.OnCreatingTicket = onCreatingTicket;
                });
            });

            var sut = provider.GetRequiredService<AuthenticationSchemeProviderWrapper>();
            Assert.Contains(typeof(FacebookHandler), sut.GetManagedHandlerTypes());

            var facebookOptions = new FacebookOptions
            {
                AppId = "test",
                AppSecret = "test"
            };

            var scheme = Guid.NewGuid().ToString();
            var definition = new TSchemeDefinition
            {
                Scheme = scheme,
                DisplayName = "test",
                HandlerType = typeof(FacebookHandler),
                Options = facebookOptions
            };

            await sut.AddAsync(definition);
            var state = await VerifyAddedAsync<FacebookOptions>(scheme, provider);

            var httpContext = new Mock<HttpContext>().Object;
            state.options.Events.OnCreatingTicket(new OAuthCreatingTicketContext(
                new ClaimsPrincipal(),
                new AuthenticationProperties(),
                httpContext,
                state.scheme as AuthenticationScheme,
                state.options as FacebookOptions,
                new HttpClient(),
                OAuthTokenResponse.Failed(new Exception()),
                JsonDocument.Parse("{ \"name\": \"test\"}").RootElement));

            Assert.True(eventCalled);
        }

        /// <summary>
        /// AddAsync method should add google handler.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddAsync_should_add_google_handler()
        {
            var eventCalled = false;
            Task onCreatingTicket(OAuthCreatingTicketContext context)
            {
                eventCalled = true;
                return Task.CompletedTask;
            }

            var provider = CreateServiceProvider(options =>
            {
                options.AddGoogle(configure =>
                {
                    configure.Events.OnCreatingTicket = onCreatingTicket;
                });
            });

            var sut = provider.GetRequiredService<AuthenticationSchemeProviderWrapper>();
            Assert.Contains(typeof(GoogleHandler), sut.GetManagedHandlerTypes());

            var googleOptions = new GoogleOptions
            {
                ClientId = "test",
                ClientSecret = "test"
            };

            var scheme = Guid.NewGuid().ToString();
            var definition = new TSchemeDefinition
            {
                Scheme = scheme,
                DisplayName = "test",
                HandlerType = typeof(GoogleHandler),
                Options = googleOptions
            };
            await sut.AddAsync(definition);
            var state = await VerifyAddedAsync<GoogleOptions>(scheme, provider);

            var httpContext = new Mock<HttpContext>().Object;
            state.options.Events.OnCreatingTicket(new OAuthCreatingTicketContext(
                new ClaimsPrincipal(),
                new AuthenticationProperties(),
                httpContext,
                state.scheme as AuthenticationScheme,
                state.options as GoogleOptions,
                new HttpClient(),
                OAuthTokenResponse.Failed(new Exception()),
                JsonDocument.Parse("{ \"name\": \"test\"}").RootElement));

            Assert.True(eventCalled);
        }

        /// <summary>
        /// AddAsync method should add jwtbearer handler.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddAsync_should_add_jwtbearer_handler()
        {
            var eventCalled = false;
            Task onMessageReceived(Microsoft.AspNetCore.Authentication.JwtBearer.MessageReceivedContext context)
            {
                eventCalled = true;
                return Task.CompletedTask;
            }

            var provider = CreateServiceProvider(options =>
            {
                options.AddJwtBearer(configure =>
                {
                    configure.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = onMessageReceived
                    };
                });
            });

            var jwtBearerOptions = new JwtBearerOptions
            {
                Authority = "test",
                RequireHttpsMetadata = false
            };

            var scheme = Guid.NewGuid().ToString();
            var definition = new TSchemeDefinition
            {
                Scheme = scheme,
                DisplayName = "test",
                HandlerType = typeof(JwtBearerHandler),
                Options = jwtBearerOptions
            };

            var sut = provider.GetRequiredService<AuthenticationSchemeProviderWrapper>();
            Assert.Contains(typeof(JwtBearerHandler), sut.GetManagedHandlerTypes());

            await sut.AddAsync(definition);
            var state = await VerifyAddedAsync<JwtBearerOptions>(scheme, provider);

            var httpContext = new Mock<HttpContext>().Object;
            state.options.Events.OnMessageReceived(new Microsoft.AspNetCore.Authentication.JwtBearer.MessageReceivedContext(
                httpContext,
                state.scheme as AuthenticationScheme,
                state.options as JwtBearerOptions));

            Assert.True(eventCalled);
        }

        /// <summary>
        /// AddAsync method should add ms account handler.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddAsync_should_add_ms_account_handler()
        {
            var eventCalled = false;
            Task onCreatingTicket(OAuthCreatingTicketContext context)
            {
                eventCalled = true;
                return Task.CompletedTask;
            }

            var provider = CreateServiceProvider(options =>
            {
                options.AddMicrosoftAccount(configure =>
                {
                    configure.Events.OnCreatingTicket = onCreatingTicket;
                });
            });

            var msAccountOptions = new MicrosoftAccountOptions
            {
                ClientId = "test",
                ClientSecret = "test"
            };

            var scheme = Guid.NewGuid().ToString();
            var definition = new TSchemeDefinition
            {
                Scheme = scheme,
                DisplayName = "test",
                HandlerType = typeof(MicrosoftAccountHandler),
                Options = msAccountOptions
            };

            var sut = provider.GetRequiredService<AuthenticationSchemeProviderWrapper>();
            Assert.Contains(typeof(MicrosoftAccountHandler), sut.GetManagedHandlerTypes());

            await sut.AddAsync(definition);
            var state = await VerifyAddedAsync<MicrosoftAccountOptions>(scheme, provider);

            var httpContext = new Mock<HttpContext>().Object;
            state.options.Events.OnCreatingTicket(new OAuthCreatingTicketContext(
                new ClaimsPrincipal(),
                new AuthenticationProperties(),
                httpContext,
                state.scheme as AuthenticationScheme,
                state.options as MicrosoftAccountOptions,
                new HttpClient(),
                OAuthTokenResponse.Failed(new Exception()),
                JsonDocument.Parse("{ \"name\": \"test\"}").RootElement));

            Assert.True(eventCalled);
        }

        /// <summary>
        /// AddAsync method should add oidc handler.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddAsync_should_add_oidc_handler()
        {
            var eventCalled = false;
            Task onTicketReceived(TicketReceivedContext context)
            {
                eventCalled = true;
                return Task.CompletedTask;
            }

            var provider = CreateServiceProvider(options =>
            {
                options.AddOpenIdConnect(configure =>
                {
                    configure.Events.OnTicketReceived = onTicketReceived;
                });
            });

            var oidcptions = new OpenIdConnectOptions
            {
                ClientId = "test",
                ClientSecret = "test"
            };

            var scheme = Guid.NewGuid().ToString();
            var definition = new TSchemeDefinition
            {
                Scheme = scheme,
                DisplayName = "test",
                HandlerType = typeof(OpenIdConnectHandler),
                Options = oidcptions
            };

            var sut = provider.GetRequiredService<AuthenticationSchemeProviderWrapper>();
            Assert.Contains(typeof(OpenIdConnectHandler), sut.GetManagedHandlerTypes());

            await sut.AddAsync(definition);
            var state = await VerifyAddedAsync<OpenIdConnectOptions>(scheme, provider);

            var httpContext = new Mock<HttpContext>().Object;
            state.options.Events.OnTicketReceived(new TicketReceivedContext(
                httpContext,
                state.scheme as AuthenticationScheme,
                state.options as OpenIdConnectOptions,
                new AuthenticationTicket(new ClaimsPrincipal(), "test")));

            Assert.True(eventCalled);
        }

        /// <summary>
        /// AddAsync method should add twitter handler.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddAsync_should_add_twitter_handler()
        {
            var eventCalled = false;
            Task onTicketReceived(TicketReceivedContext context)
            {
                eventCalled = true;
                return Task.CompletedTask;
            }

            var provider = CreateServiceProvider(options =>
            {
                options.AddTwitter(configure =>
                {
                    configure.Events.OnTicketReceived = onTicketReceived;
                });
            });

            var twittertOptions = new TwitterOptions
            {
                ConsumerKey = "test",
                ConsumerSecret = "test"
            };

            var scheme = Guid.NewGuid().ToString();
            var definition = new TSchemeDefinition
            {
                Scheme = scheme,
                DisplayName = "test",
                HandlerType = typeof(TwitterHandler),
                Options = twittertOptions
            };

            var sut = provider.GetRequiredService<AuthenticationSchemeProviderWrapper>();
            Assert.Contains(typeof(TwitterHandler), sut.GetManagedHandlerTypes());

            await sut.AddAsync(definition);
            var state = await VerifyAddedAsync<TwitterOptions>(scheme, provider);

            var httpContext = new Mock<HttpContext>().Object;
            state.options.Events.OnTicketReceived(new TicketReceivedContext(
                httpContext,
                state.scheme as AuthenticationScheme,
                state.options as TwitterOptions,
                new AuthenticationTicket(new ClaimsPrincipal(), "test")));

            Assert.True(eventCalled);
        }

        /// <summary>
        /// AddAsync method should add ws federation handler.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddAsync_should_add_ws_federation_handler()
        {
            var eventCalled = false;
            Task onTicketReceived(TicketReceivedContext context)
            {
                eventCalled = true;
                return Task.CompletedTask;
            }

            var provider = CreateServiceProvider(options =>
            {
                options.AddWsFederation(configure =>
                {
                    configure.Events.OnTicketReceived = onTicketReceived;
                });
            });

            var wsFederationOptions = new WsFederationOptions
            {
                Configuration = new WsFederationConfiguration
                {
                    Issuer = "test"
                }
            };

            var scheme = Guid.NewGuid().ToString();
            var definition = new TSchemeDefinition
            {
                Scheme = scheme,
                DisplayName = "test",
                HandlerType = typeof(WsFederationHandler),
                Options = wsFederationOptions
            };

            var sut = provider.GetRequiredService<AuthenticationSchemeProviderWrapper>();
            Assert.Contains(typeof(WsFederationHandler), sut.GetManagedHandlerTypes());

            await sut.AddAsync(definition);
            var state = await VerifyAddedAsync<WsFederationOptions>(scheme, provider);

            var httpContext = new Mock<HttpContext>().Object;
            state.options.Events.OnTicketReceived(new TicketReceivedContext(
                httpContext,
                state.scheme as AuthenticationScheme,
                state.options as WsFederationOptions,
                new AuthenticationTicket(new ClaimsPrincipal(), scheme)));

            Assert.True(eventCalled);
        }

        /// <summary>
        /// AddAsync method should add generic handler.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddAsync_should_add_generic_handler()
        {
            var eventCalled = false;
            Task onTicketReceived(TicketReceivedContext context)
            {
                eventCalled = true;
                return Task.CompletedTask;
            }

            var provider = CreateServiceProvider(options =>
            {
                options.AddScheme<OAuthOptions, FakeGenericHandler<OAuthOptions>>("test", configure =>
                {
                    configure.Events.OnTicketReceived = onTicketReceived;
                });
            });

            var oAuthOptions = new OAuthOptions
            {
                ClientId = "test"                
            };

            var scheme = Guid.NewGuid().ToString();
            var definition = new TSchemeDefinition
            {
                Scheme = scheme,
                DisplayName = "test",
                HandlerType = typeof(FakeGenericHandler<OAuthOptions>),
                Options = oAuthOptions
            };

            var sut = provider.GetRequiredService<AuthenticationSchemeProviderWrapper>();
            Assert.Contains(typeof(FakeGenericHandler<OAuthOptions>), sut.GetManagedHandlerTypes());

            await sut.AddAsync(definition);
            var state = await VerifyAddedAsync<OAuthOptions>(scheme, provider);

            var httpContext = new Mock<HttpContext>().Object;
            state.options.Events.OnTicketReceived(new TicketReceivedContext(
                httpContext,
                state.scheme as AuthenticationScheme,
                state.options as OAuthOptions,
                new AuthenticationTicket(new ClaimsPrincipal(), scheme)));

            Assert.True(eventCalled);
        }


        /// <summary>
        /// UpdateAsync method should update handler.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateAsync_should_update_handler()
        {
            var eventCalled = false;
            Task onTicketReceived(TicketReceivedContext context)
            {
                eventCalled = true;
                return Task.CompletedTask;
            }

            var provider = CreateServiceProvider(options =>
            {
                options.AddCookie()
                    .AddWsFederation(configure =>
                    {
                        configure.Events.OnTicketReceived = onTicketReceived;
                    });
            });

            var cookieOptions = new CookieAuthenticationOptions
            {
                Cookie = new CookieBuilder
                {
                    Domain = "test"
                }
            };

            var scheme = Guid.NewGuid().ToString();
            var definition = new TSchemeDefinition
            {
                Scheme = scheme,
                DisplayName = "test",
                HandlerType = typeof(CookieAuthenticationHandler),
                Options = cookieOptions
            };

            var sut = provider.GetRequiredService<AuthenticationSchemeProviderWrapper>();
            Assert.Contains(typeof(CookieAuthenticationHandler), sut.GetManagedHandlerTypes());
            Assert.Contains(typeof(WsFederationHandler), sut.GetManagedHandlerTypes());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.UpdateAsync(definition));

            await sut.AddAsync(definition);
            await VerifyAddedAsync<CookieAuthenticationOptions>(scheme, provider);

            var wsFederationOptions = new WsFederationOptions
            {
                Configuration = new WsFederationConfiguration
                {
                    Issuer = "test"
                }
            };

            definition.Options = wsFederationOptions;
            definition.DisplayName = "new name";
            definition.HandlerType = typeof(WsFederationHandler);

            await sut.UpdateAsync(definition);
            var state = await VerifyAddedAsync<WsFederationOptions>(scheme, provider);

            Assert.Equal(state.scheme.DisplayName, definition.DisplayName);

            var httpContext = new Mock<HttpContext>().Object;
            state.options.Events.OnTicketReceived(new TicketReceivedContext(
                httpContext,
                state.scheme as AuthenticationScheme,
                state.options as WsFederationOptions,
                new AuthenticationTicket(new ClaimsPrincipal(), scheme)));

            Assert.True(eventCalled);
        }

        /// <summary>
        /// RemoveAsync method should remove handler.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RemoveAsync_should_remove_handler()
        {
            var provider = CreateServiceProvider(options =>
            {
                options.AddCookie();
            });

            var cookieOptions = new CookieAuthenticationOptions
            {
                Cookie = new CookieBuilder
                {
                    Domain = "test"
                }
            };

            var scheme = Guid.NewGuid().ToString();
            var definition = new TSchemeDefinition
            {
                Scheme = scheme,
                DisplayName = "test",
                HandlerType = typeof(CookieAuthenticationHandler),
                Options = cookieOptions
            };

            var sut = provider.GetRequiredService<AuthenticationSchemeProviderWrapper>();
            Assert.Contains(typeof(CookieAuthenticationHandler), sut.GetManagedHandlerTypes());

            await sut.AddAsync(definition);
            await VerifyAddedAsync<CookieAuthenticationOptions>(scheme, provider);

            await sut.RemoveAsync(scheme);
            await Assert.ThrowsAsync<NotNullException>(() => VerifyAddedAsync<WsFederationOptions>(scheme, provider));
        }

        /// <summary>
        /// Loads the should load configuration.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Load_should_load_configuration()
        {
            var provider = CreateServiceProvider(options =>
            {
                options.AddCookie();
            });

            var cookieOptions = new CookieAuthenticationOptions
            {
                Cookie = new CookieBuilder
                {
                    Domain = "test"
                }
            };

            var scheme = Guid.NewGuid().ToString();
            var definition = new TSchemeDefinition
            {
                Scheme = scheme,
                HandlerType = typeof(CookieAuthenticationHandler),
                Options = cookieOptions
            };

            var sut = provider.GetRequiredService<AuthenticationSchemeProviderWrapper>();

            await sut.InitializeAsync(new[] { (ISchemeDefinition)definition });

            await VerifyAddedAsync<CookieAuthenticationOptions>(scheme, provider);
        }

        /// <summary>
        /// Adds the store.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        protected abstract DynamicAuthenticationBuilder AddStore(DynamicAuthenticationBuilder builder);

        private async Task<dynamic> VerifyAddedAsync<TOptions>(string schemeName, IServiceProvider provider) where TOptions : AuthenticationSchemeOptions
        {
            var schemeProvider = provider.GetRequiredService<IAuthenticationSchemeProvider>();
            var scheme = await schemeProvider.GetSchemeAsync(schemeName);
            Assert.NotNull(scheme);

            var optionsMonitorCache = provider.GetRequiredService<IOptionsMonitorCache<TOptions>>();
            var options = optionsMonitorCache.GetOrAdd(schemeName, () => default);
            Assert.NotNull(options);

            return new { scheme, options };
        }

        /// <summary>
        /// Creates the service providers
        /// </summary>
        /// <param name="addHandlers">A function to invoke before returning the provider</param>
        /// <returns></returns>
        protected virtual IServiceProvider CreateServiceProvider(Action<AuthenticationBuilder> addHandlers = null)
        {
            var services = new ServiceCollection();
            var builder = services.AddLogging(configure =>
            {
                configure.AddConsole()
                    .AddDebug();
            })
                .AddAuthentication()
                .AddDynamicAuthentication();

            AddStore(builder);

            addHandlers?.Invoke(builder);

            return services.BuildServiceProvider();
        }

        class FakeGenericHandler<TOptions> : AuthenticationHandler<TOptions>
            where TOptions : AuthenticationSchemeOptions, new()
        {
            public FakeGenericHandler(IOptionsMonitor<TOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
            {
            }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                return Task.FromResult(new FakeAuthenticateResult() as AuthenticateResult);
            }

            public class FakeAuthenticateResult: AuthenticateResult
            {
                public FakeAuthenticateResult() : base()
                {
                }
            }
        }
    }
}
