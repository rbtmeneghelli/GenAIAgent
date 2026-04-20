using System;
using System.Collections.Generic;
using System.Text;

namespace GenAIAgent;

public interface IGenAIFactory
{
    Task CreateAgent();
    Task CreateAgent_V1(IConfiguration config);
    Task CreateAgent_V2(IConfiguration config);
}
