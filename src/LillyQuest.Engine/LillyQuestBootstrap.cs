using DryIoc;
using LillyQuest.Core.Data.Contexts;
using Serilog;

namespace LillyQuest.Engine;

public class LillyQuestBootstrap
{

    private readonly ILogger _logger = Log.ForContext<LillyQuestBootstrap>();

    private readonly IContainer _container = new Container();
    private EngineRenderContext _renderContext;

    public void RegisterServices(Func<IContainer,IContainer> registerServices)
    {
    }



}
