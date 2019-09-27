using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SentimentBot
{
  public class Worker : BackgroundService
  {

    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _Config;
    private ChatBot _Bot;


    public Worker(IConfiguration config, ILogger<Worker> logger)
    {
      _logger = logger;
      _Config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {

        _Bot = new ChatBot(_Config, _logger);
        await _Bot.Start(stoppingToken);

      }
    }
  }
}
