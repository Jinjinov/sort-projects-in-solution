using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SortProjectsInSolutionFile
{
    class Project
    {
        public string line;
        public string type;
        public string name;
        public string file;
        public string guid;
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
                return;

            string path = args[0];
            string newPath = path + ".new";

            using StreamWriter stream = new StreamWriter(newPath);

            List<Project> projectList = new List<Project>();
            bool writeProjects = false;

            List<string> projectConfigurationList = new List<string>();
            bool projectConfigurationPlatforms = false;

            List<string> nestedProjectsList = new List<string>();
            bool nestedProjects = false;

            foreach (string line in File.ReadLines(path))
            {
                bool isProject = line.StartsWith("Project");
                bool isEndProject = line.StartsWith("EndProject");

                if (isProject)
                {
                    List<int> indexList = new List<int>();

                    for (int i = 0; i < line.Length; i++)
                    {
                        if (line[i] == '"')
                        {
                            indexList.Add(i);
                        }
                    }

                    projectList.Add(new Project
                    {
                        line = line,
                        type = line[(indexList[0] + 1)..indexList[1]],
                        name = line[(indexList[2] + 1)..indexList[3]],
                        file = line[(indexList[4] + 1)..indexList[5]],
                        guid = line[(indexList[6] + 1)..indexList[7]]
                    });

                    writeProjects = true;
                }
                else if(!isEndProject)
                {
                    if (projectConfigurationPlatforms)
                    {
                        if (line.Contains("EndGlobalSection"))
                        {
                            foreach (Project project in projectList.OrderBy(p => p.name).ThenBy(p => p.type))
                            {
                                foreach (string projectConfiguration in projectConfigurationList.Where(p => p[p.IndexOf('{')..(p.IndexOf('}') + 1)] == project.guid))
                                {
                                    stream.WriteLine(projectConfiguration);
                                }
                            }

                            stream.WriteLine(line);

                            projectConfigurationPlatforms = false;
                        }
                        else
                        {
                            projectConfigurationList.Add(line);
                        }
                    }
                    else if (nestedProjects)
                    {
                        if (line.Contains("EndGlobalSection"))
                        {
                            foreach (Project project in projectList.OrderBy(p => p.name).ThenBy(p => p.type))
                            {
                                foreach (string nestedProject in nestedProjectsList.Where(p => p[p.IndexOf('{')..(p.IndexOf('}') + 1)] == project.guid))
                                {
                                    stream.WriteLine(nestedProject);
                                }
                            }

                            stream.WriteLine(line);

                            nestedProjects = false;
                        }
                        else
                        {
                            nestedProjectsList.Add(line);
                        }
                    }
                    else
                    {
                        if (line.Contains("ProjectConfigurationPlatforms"))
                        {
                            projectConfigurationPlatforms = true;
                        }

                        if (line.Contains("NestedProjects"))
                        {
                            nestedProjects = true;
                        }

                        if (writeProjects)
                        {
                            foreach (Project project in projectList.OrderBy(p => p.name).ThenBy(p => p.type))
                            {
                                stream.WriteLine(project.line);
                                stream.WriteLine("EndProject");
                            }

                            writeProjects = false;
                        }

                        stream.WriteLine(line);
                    }
                }
            }

            stream.Close();

            Console.WriteLine("Original file length: " + new FileInfo(path).Length);
            Console.WriteLine("- Sorted file length: " + new FileInfo(newPath).Length);

            Console.WriteLine("Original file characters: " + File.ReadAllLines(path).Sum(s => s.Length));
            Console.WriteLine("- Sorted file characters: " + File.ReadAllLines(newPath).Sum(s => s.Length));

            Console.ReadLine();
        }
    }
}
