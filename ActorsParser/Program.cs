using ActorsParser.Details;
using ActorsParser;
using Microsoft.EntityFrameworkCore.Query;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.EntityFrameworkCore;
using ActorsParser;

public class Program
{
    static string path = "C:/postgre/actors.list.txt";
    public static void Main()
    {
        List<Actor> actors = new List<Actor>();
        using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (BufferedStream bs = new BufferedStream(fs))
        using (StreamReader sr = new StreamReader(bs))
        {
            sr.ReadLine(); sr.ReadLine(); // skip 2 first lines
            string? block = "";
            string? line = sr.ReadLine();
            int id = 1;
            while (line != null)
            {
                if (line == "")
                {
                    Actor actor = new Actor();

                    var parts = block.Split('\t');
                    actor.Id = id;
                    Regex reg = new Regex(@"(?i)[^А-ЯЁA-Z\s']");
                    var name_parts = reg.Replace(parts[0], "").Trim().Split(' ');
                    foreach (var np in name_parts.Reverse()) actor.Name += np + ' ';
                    actor.Name = actor.Name.Trim();

                    Regex t_regex = new Regex(@"\A[IVX]+\s");
                    actor.Name = t_regex.Replace(actor.Name, "").Trim();
                    actor.Roles.Add("roles", new List<Role>());

                    for (int i = 1; i < parts.Length; ++i)
                    {
                        Role role = new Role();
                        int type_c = 1;
                        if (parts[i] != "")
                        {
                            string pattern_film_name = @"\A.*\(";
                            string patternBrackets = @"\([^)]+\)";
                            string patternSquareBrackets = @"\[[^)]+\]";
                            string patternFigureBrackets = @"\{[^)]+\}";
                            string patternTriangleBrackets = @"\<[^)]+\>";
                            var options = RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled;

                            var match = Regex.Match(parts[i], pattern_film_name, options);
                            role.Title = match.Value.Split('(')[0].Trim();

                            var matches = Regex.Matches(parts[i], patternSquareBrackets, options);
                            if (matches.Count != 0)
                            {
                                role.Character_Name = MyTrim(matches[0].Value);
                            }
                            matches = Regex.Matches(parts[i], patternFigureBrackets, options);
                            if (matches.Count != 0)
                            {
                                role.Series_Name = MyTrim(matches[0].Value);
                                parts[i] = parts[i].Replace(role.Series_Name, "");
                            }
                            matches = Regex.Matches(parts[i], patternBrackets, options);
                            if (matches.Count != 0)
                            {
                                role.Year = MyTrim(matches[0].Value);
                                if (matches.Count > 1)
                                {
                                    for (int j = 1; j < matches.Count; ++j)
                                    {
                                        if (matches[j].Value == "(V)" || matches[j].Value == "(TV)"
                                            || matches[j].Value == "(VG)" || matches[j].Value == "(archive footage)"
                                            || matches[j].Value == "(uncredited)" || matches[j].Value == "(voice)")
                                        {
                                            switch(type_c)
                                            {
                                                case 1:
                                                    role.type1 = MyTrim(matches[j].Value);
                                                    break;
                                                case 2:
                                                    role.type2 = MyTrim(matches[j].Value);
                                                    break;
                                                case 3:
                                                    role.type3 = MyTrim(matches[j].Value);
                                                    break;
                                                case 4:
                                                    role.type4 = MyTrim(matches[j].Value);
                                                    break;
                                                case 5:
                                                    role.type5 = MyTrim(matches[j].Value);
                                                    break;
                                                case 6:
                                                    role.type6 = MyTrim(matches[j].Value);
                                                    break;
                                            }
                                            ++type_c;
                                        }
                                        else
                                        {
                                            role.Character_Name += ' ' + MyTrim(matches[j].Value);
                                        }
                                    }
                                }
                            }
                            matches = Regex.Matches(parts[i], patternTriangleBrackets, options);
                            if (matches.Count != 0)
                            {
                                role.Credit = MyTrim(matches[0].Value);
                            }
                            actor.Roles["roles"].Add(role);
                        }
                    }
                    actors.Add(actor);
                    block = null;
                    ++id;
                }
                else block += line;

                line = sr.ReadLine();
            }
        }

        using (ApplicationContext db = new ApplicationContext())
        {
            db.Actors.AddRange(actors.ToArray());
            db.SaveChanges();
        }
    }

    public static string MyTrim(string str)
    {
        return str.Substring(1, str.Length - 2);
    }
}
