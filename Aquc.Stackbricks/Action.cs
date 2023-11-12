using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aquc.Stackbricks.Actions;

namespace Aquc.Stackbricks;


public class UpdateActionData
{
    public string Id;
    public List<string> Args;
    public List<string> Flags;
    public UpdateActionData(string id, List<string> args, List<string> flags)
    {
        Id = id;
        Args = args;
        Flags = flags;
    }
    public UpdateActionData(string id)
    {
        Id = id;
        Args = new List<string>();
        Flags = new List<string>();
    }
    public UpdateActionData(IUpdateAction action) : this(action.ActionId) { }
    public bool ContainFlag(string i) => Flags.Contains(i);
}
public class UpdateActionManager
{
    static readonly Dictionary<string, IUpdateAction> DefaultActions = new()
    {
        { ActionOpen.ID, new ActionOpen()},
        { ActionReplaceAll.ID,new ActionReplaceAll() },
        { ActionRunUpdatePackageActions.ID,new ActionRunUpdatePackageActions() },
        { ActionApplySelfUpdate.ID,new ActionApplySelfUpdate() },
        { ActionExecuteCommand.ID,new ActionExecuteCommand() },
    };
    public Dictionary<string, IUpdateAction> Actions;
    public UpdateActionManager()
    {
        Actions = DefaultActions;
    }
    public UpdateActionManager(Dictionary<string, IUpdateAction> actions)
    {
        Actions = DefaultActions.Concat(actions).ToDictionary(x => x.Key, x => x.Value);
    }
    public static IUpdateAction ParseStatic(string id)
    {
        if (DefaultActions.TryGetValue(id, out IUpdateAction? value)) return value;
        else throw new ArgumentException();
    }
    public IUpdateAction Parse(string id, UpdateActionData stackbricksAction)
    {
        if (Actions.TryGetValue(id, out IUpdateAction? value))
            return value;
        else
            throw new ArgumentException();
    }

}
public class UpdateActionList
{
    public class UpdateActionListConfig
    {
        public List<UpdateActionData> actions = new();
    }
    public List<UpdateActionData> actions;
    public UpdateActionList(string PkgConfigFile)
    {
        using var fs = new FileStream(PkgConfigFile, FileMode.Open, FileAccess.Read);
        using var sr = new StreamReader(fs);
        actions = JsonConvert.DeserializeObject<UpdateActionListConfig>(sr.ReadToEnd(), StackbricksProgram.jsonSerializer)!.actions;
    }
    public UpdateActionList(List<UpdateActionData> list)
    {
        actions = list;
    }
    public void ExecuteList(UpdatePackage updatePackage)
    {
        StackbricksProgram.logger.Debug($"Found {actions.Count} update actions.");
        foreach (var actionData in actions)
        {
            var action = UpdateActionManager.ParseStatic(actionData.Id);
            StackbricksProgram.logger.Debug($"Execute {actionData.Id}.");
            action.Execute(actionData, updatePackage);
        }
    }
}
public interface IUpdateAction
{
    public string ActionId { get; }
    public void Execute(UpdateActionData stackbricksAction, UpdatePackage updatePackage);
}
public class ActionOpen : IUpdateAction
{
    public string ActionId => ID;
    public const string ID = "stbks.action.open";
    public void Execute(UpdateActionData stackbricksAction, UpdatePackage updatePackage)
    {

    }
}
public class ActionRunUpdatePackageActions : IUpdateAction
{

    public string ActionId => ID;
    public const string ID = "stbks.action.runupdpkgactions";
    public void Execute(UpdateActionData stackbricksAction, UpdatePackage updatePackage)
    {
    }
}