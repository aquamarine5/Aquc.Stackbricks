using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aquc.Stackbricks.Actions;

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
    public StackbricksActionData(string id)
    {
        Id = id;
        Args = new List<string>();
        Flags = new List<string>();
    }
    public StackbricksActionData(IStackbricksAction action) : this(action.ActionId) { }
    public bool ContainFlag(string i) => Flags.Contains(i);
}
public class StackbricksActionManager
{
    static readonly Dictionary<string, IStackbricksAction> DefaultActions = new()
    {
        { ActionOpen.ID, new ActionOpen()},
        { ActionReplaceAll.ID,new ActionReplaceAll() },
        { ActionRunUpdatePackageActions.ID,new ActionRunUpdatePackageActions() },
        {ActionApplySelfUpdate.ID,new ActionApplySelfUpdate() }
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
    public static IStackbricksAction ParseStatic(string id)
    {
        if (DefaultActions.TryGetValue(id, out IStackbricksAction? value)) return value;
        else throw new ArgumentException();
    }
    public IStackbricksAction Parse(string id, StackbricksActionData stackbricksAction)
    {
        if (Actions.TryGetValue(id, out IStackbricksAction? value))
            return value;
        else
            throw new ArgumentException();
    }

}
public class StackbricksActionList
{
    public class StackbricksActionListConfig
    {
        public List<StackbricksActionData> actions = new();
    }
    public List<StackbricksActionData> actions;
    public StackbricksActionList(string PkgConfigFile)
    {
        using var fs = new FileStream(PkgConfigFile, FileMode.Open, FileAccess.Read);
        using var sr = new StreamReader(fs);
        actions = JsonConvert.DeserializeObject<StackbricksActionListConfig>(sr.ReadToEnd(), StackbricksProgram.jsonSerializer)!.actions;
    }
    public StackbricksActionList(List<StackbricksActionData> list)
    {
        actions = list;
    }
    public void ExecuteList(StackbricksUpdatePackage updatePackage)
    {
        StackbricksProgram.logger.Debug($"Found {actions.Count} update actions.");
        foreach (var actionData in actions)
        {
            var action = StackbricksActionManager.ParseStatic(actionData.Id);
            StackbricksProgram.logger.Debug($"Execute {actionData.Id}.");
            action.Execute(actionData, updatePackage);
        }
    }
}
public interface IStackbricksAction
{
    public string ActionId { get; }
    public void Execute(StackbricksActionData stackbricksAction, StackbricksUpdatePackage updatePackage);
}
public class ActionOpen : IStackbricksAction
{
    public string ActionId => ID;
    public const string ID = "stbks.action.open";
    public void Execute(StackbricksActionData stackbricksAction, StackbricksUpdatePackage updatePackage)
    {

    }
}
public class ActionRunUpdatePackageActions : IStackbricksAction
{

    public string ActionId => ID;
    public const string ID = "stbks.action.runupdpkgactions";
    public void Execute(StackbricksActionData stackbricksAction, StackbricksUpdatePackage updatePackage)
    {
    }
}