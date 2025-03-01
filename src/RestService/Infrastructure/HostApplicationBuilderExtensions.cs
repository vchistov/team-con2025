namespace RestService.Infrastructure;

using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddGrpcClient<TClient>(this IHostApplicationBuilder builder) where TClient : class
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.TryAddSingleton<AcceptLanguageInterceptor>();

        builder.Services.AddGrpcClient<TClient>(opt =>
        {
            opt.Address = new("http://grpc-svc");
        }).ConfigureChannel(opt =>
        {
            opt.UnsafeUseInsecureChannelCallCredentials = true;
            opt.ServiceConfig = new ServiceConfig
            {
                RetryThrottling = new RetryThrottlingPolicy
                {
                    MaxTokens = 6,
                    TokenRatio = 0.5,
                },
                MethodConfigs = { CreateDefaultMethodConfig() }
            };
        }).AddCallCredentials((_, metadata, sp) =>
        {
            var apiKey = builder.Configuration.GetValue<string>("GrpcSvc:ApiKey")!;
            metadata.Add("X-Api-Key", apiKey);
            
            return Task.CompletedTask;
        }).AddInterceptor<AcceptLanguageInterceptor>();

        return builder;
    }

    private static MethodConfig CreateDefaultMethodConfig()
    {
        return new MethodConfig
        {
            Names = { MethodName.Default },
            RetryPolicy = GetRetryPolicy(),
        };
    }

    private static RetryPolicy GetRetryPolicy()
    {
        return new RetryPolicy
        {
            MaxAttempts = 3,
            InitialBackoff = TimeSpan.FromMilliseconds(500),
            MaxBackoff = TimeSpan.FromSeconds(3),
            BackoffMultiplier = 0.5,
            RetryableStatusCodes = { StatusCode.Unavailable }
        };
    }

    private class AcceptLanguageInterceptor(IHttpContextAccessor httpContextAccessor) : Interceptor
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            AddAcceptLanguage(ref context);
            return base.AsyncUnaryCall(request, context, continuation);
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            AddAcceptLanguage(ref context);
            return base.BlockingUnaryCall(request, context, continuation);
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            AddAcceptLanguage(ref context);
            return base.AsyncClientStreamingCall(context, continuation);
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            AddAcceptLanguage(ref context);
            return base.AsyncDuplexStreamingCall(context, continuation);
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            AddAcceptLanguage(ref context);
            return base.AsyncServerStreamingCall(request, context, continuation);
        }

        private void AddAcceptLanguage<TRequest, TResponse>(ref ClientInterceptorContext<TRequest, TResponse> context)
            where TRequest : class
            where TResponse : class
        {
            if (string.IsNullOrEmpty(httpContextAccessor?.HttpContext?.Request.Headers.AcceptLanguage))
            {
                return;            
            }

            var headers = context.Options.Headers;

            // Call doesn't have a headers collection to add to.
            // Need to create a new context with headers for the call.
            if (headers == null)
            {
                headers = new Metadata();
                var options = context.Options.WithHeaders(headers);
                context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);
            }

            // Add caller's language preferences to headers
            headers.Add("Accept-Language", httpContextAccessor.HttpContext.Request.Headers.AcceptLanguage!);
        }
    }
}
