namespace Octgn.ReleasePusher.Tasking.Tasks
{
    using System;

    public class CreatePushBatFile: ITask
    {
        public void Run(object sender, ITaskContext context)
        {
            const string BatTemplate = @"git commit -am ""Auto generated commit for version %v""
git tag -a %v -m ""Auto generated commit for version %v""
git push origin HEAD:master
git push --tags origin";
            var file = context.Data["WorkingDirectory"] as string;
            file = context.FileSystem.Path.Combine(file, "push.bat");

            var outstr = BatTemplate.Replace("%v", (context.Data["NewVersion"] as Version).ToString());

            context.FileSystem.File.WriteAllText(file,outstr);
        }
    }
}
