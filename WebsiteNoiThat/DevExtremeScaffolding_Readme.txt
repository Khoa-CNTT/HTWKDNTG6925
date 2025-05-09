DevExtreme Scaffolder has added a Web API controller to project 'WebsiteNoiThat'. 

However, the Global.asax.cs file in the project may require additional changes to enable ASP.NET Web API:

1. Add the following namespace references:

    using System.Web.Http;
    using System.Web.Routing;

2. If the code does not already define an Application_Start method, add it:

    protected void Application_Start()
    {
    }

3. Add the following lines to the beginning of the Application_Start method:

    GlobalConfiguration.Configure(WebApiConfig.Register);