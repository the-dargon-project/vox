var projects = Projects("vox");
var dependencies = Projects("commons", "ryu", "NMockito");

Export.Solution(
   Name: "Dargon Messages",
   Commands: new ICommand[] {
      Build.Projects(projects, dependencies),
      Test.Projects(projects)
   }
);
