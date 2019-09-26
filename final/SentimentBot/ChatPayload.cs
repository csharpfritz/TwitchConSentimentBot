using System;
using System.Collections.Generic;
using System.Text;

namespace SentimentBot
{
  public class ChatPayload
  {

    public string Channel { get; set; }

    public string UserName { get; set; }

    public string SentimentText { get; set; }

  }
}
