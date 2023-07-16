using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquc.Stackbricks;


public class StackbricksActionData
{
    public string Id;
    public List<string> Args;
    public List<string> Flags;
    public StackbricksActionData(string id, List<string> args, List<string> flags)
    {
        Id = id;
        Args = args;
        Flags = flags;
    }
}
public class StackbricksActionManager
{
    static readonly Dictionary<string, IStackbricksAction> DefaultActions=new()
    {
        { "stbks.action.open", new ActionOpen()},
        {"stbks.action.replaceall",new ActionReplaceAll() },
        {"stbks.action.runupdpkgactions",new ActionRunUpdatePackageActions() }
    };
    public Dictionary<string, IStackbricksAction> Actions;
    public StackbricksActionManager() 
    {
        Actions = DefaultActions;
    }
    public StackbricksActionManager(Dictionary<string, IStackbricksAction> actions)
    {
        Actions = DefaultActions.Concat(actions).ToDictionary(x => x.Key, x => x.Value);
    }
    public static IStackbricksAction ParseStatic(string id,StackbricksActionData stackbricksAction)
    {
        if (DefaultActions.TryGetValue(id, out IStackbricksAction? value))return value;
        else throw new ArgumentException();
    }
    public IStackbricksAction Parse(string id, StackbricksActionData stackbricksAction)
    {
        if(Actions.TryGetValue(id,out IStackbricksAction? value))
            return value;
        else
            throw new ArgumentException();
    }
}
public interface IStackbricksAction
{
    public string Id { get; }
    public void Execute(StackbricksActionData stackbricksAction);
}
public class ActionOpen : IStackbricksAction
{
    public string Id => "Action.Open";
    public void Execute(StackbricksActionData stackbricksAction)
    {

    }
}
public class ActionRunUpdatePackageActions : IStackbricksAction
{

    public string Id => "Action.RunUpdatePackageActions";
    public void Execute(StackbricksActionData stackbricksAction)
    {

    }
}
public class ActionReplaceAll : IStackbricksAction
{
    public string Id => "Action.ReplaceAll";
    public void Execute(StackbricksActionData stackbricksAction)
    {

    }
}
public abstract class StackbricksBaseAction
{
    public abstract string Id { get; }

    public List<string> Args { get;}
    public List<string> Flags { get; }
    public StackbricksBaseAction(StackbricksActionData stackbricksAction)
    {
        Args=stackbricksAction.Args;
        Flags=stackbricksAction.Flags;
    }
}