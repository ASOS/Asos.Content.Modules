namespace Asos.Content.Modules.ItemUnlocker
{
    using System;

    using Sitecore;
    using Sitecore.Diagnostics;
    using Sitecore.SecurityModel;
    using Sitecore.Shell.Framework.Commands;

    [Serializable]
    public class UnlockItemCommand : Command
    {
        private const string UnlockRole = "sitecore\\Asos Unlock Other Users Items";

        public override void Execute(CommandContext context)
        {
            var item = context.Items[0];

            if (!item.Access.CanWriteLanguage() || !item.Locking.IsLocked())
            {
                return;
            }
            var user = Context.User;

            Log.Info(string.Format("{0} has unlocked {1}", user.Name, AuditFormatter.FormatItem(item)), this);

            using (new SecurityDisabler())
            {
                item.Editing.BeginEdit();
                item.Locking.Unlock();
                item.Editing.EndEdit();
                Context.ClientPage.SendMessage(this, "item:checkedin");
            }
        }

        public override CommandState QueryState(CommandContext context)
        {
            if (!Context.User.IsInRole(UnlockRole))
            {
                return CommandState.Disabled;
            }

            var item = context.Items[0];

            if (!item.Access.CanWriteLanguage())
            {
                return CommandState.Disabled;
            }

            var itemIsLocked = item.Locking.IsLocked() && !item.Locking.HasLock();

            if (!itemIsLocked)
            {
                return CommandState.Disabled;
            }

            return CommandState.Enabled;
        }
    }
}
