using Quartz;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // base configuration from appsettings.json
        services.Configure<QuartzOptions>(hostContext.Configuration.GetSection("Quartz"));

        // if you are using persistent job store, you might want to alter some options
        services.Configure<QuartzOptions>(options =>
        {
            var jobStoreType = options["quartz.jobStore.type"];
            if ((jobStoreType ?? string.Empty) == "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz")
            {
                options.Scheduling.IgnoreDuplicates = true; // default: false
                options.Scheduling.OverWriteExistingData = true; // default: true
            }

            var dataSource = options["quartz.jobStore.dataSource"];
            if (!string.IsNullOrEmpty(dataSource))
            {
                var connectionStringName = options[$"quartz.dataSource.{dataSource}.connectionStringName"];
                var connStr = hostContext.Configuration.GetConnectionString(connectionStringName);
                options[$"quartz.dataSource.{dataSource}.connectionString"] = connStr;
            }
        });
        // Add the required Quartz.NET services
        services.AddQuartz(q =>
        {
            // Use a Scoped container to create jobs. I'll touch on this later
            q.UseMicrosoftDependencyInjectionJobFactory();

        });
        // Add the Quartz.NET hosted service
        services.AddQuartzHostedService(
            q => q.WaitForJobsToComplete = true);
    })
    .Build();

await host.RunAsync();

