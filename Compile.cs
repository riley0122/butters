using System.IO;
using System.Text.Json;

namespace butters
{
    public class META{
        public string author { get; set; } = "[Butters] Default author";
        public string project { get; set; } = "[Butters] Default project name";
        public string version { get; set; } = "[Butters] NO VERSION INFO AVAILABLE";
        public string? license { get; set; }
    }

    public class VAR{
        public enum VarType
        {
            FLOAT = 0,
            INT = 1,
            STRING = 2,
            BOOL = 3,
            FLOAT_arr = 4,
            INT_arr = 5,
            STRING_arr = 6,
            VOID = 7,
            NULL = 8,
            VAR = 2,
        }

        public string name { get; set; } = "[Butters] Default Variable Name";
        public dynamic? value { get; set; } = null;
        public VarType type { get; set; } = VarType.NULL;
    }

    class code_block{
        public string instruction { get ; set; } = "HALT";
        public string? var { get; set; }
        public string? origin { get; set; }
        public string? value { get; set; }
        public List<code_block>? runs { get; set; }
        public string? condition { get; set; } 
        public string? output { get; set; } 
    }

    class Compile
    {
        public META? _latestMETA = null;
        public List<code_block> code = new List<code_block>();

        enum SECTION
        {
            META = 0,
            _STATIC, _DYNAMIC = 1,
            DEFINE = 2,
            CODE = 3,
        }

        public void comp(string fileLoc){
            Console.ForegroundColor = ConsoleColor.Cyan;
            Program.log("[Compile.cs/comp] started compilation for " + fileLoc);
            Console.ForegroundColor = ConsoleColor.White;
            string[] file = readFile(fileLoc);
            generate(file);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Program.log("[Compile.cs/comp] finished compilation for " + fileLoc);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public code_block loopObj = new code_block();
        public bool inloop = false;
        bool dontadd = false;
        public code_block codeSwitch(string[] split, string action, int line = -1){
            code_block c = new code_block();
            dontadd = false;
            switch (action)
            {
                case "out":
                    c.instruction = "out";
                    c.var = split[1];
                    return c;
                case "input":
                    c.instruction = "input";
                    c.var = split[1];
                    c.output = split[2];
                    return c;
                case "cvar": 
                    c.origin = "cvar";
                    c.instruction = "redef";
                    c.var = split[1];
                    List<string> val = new List<string>(split);
                    val.Remove("cvar");
                    val.Remove(c.var);
                    val.Remove("/");
                    c.value = string.Join(" ", val);
                    return c;
                case "while":
                    c.instruction = "while";
                    List<string> WhileCondition = new List<string>(split);
                    WhileCondition.Remove("while");
                    c.condition = string.Join(" ", WhileCondition);
                    inloop = true;
                    c.runs = new List<code_block>();
                    loopObj = c;
                    return c;
                case "if":
                    c.instruction = "if";
                    List<string> IfCondition = new List<string>(split);
                    IfCondition.Remove("if");
                    c.condition = string.Join(" ", IfCondition);
                    inloop = true;
                    c.runs = new List<code_block>();
                    loopObj = c;
                    return c;
                case "$":
                    List<string> newSplit = new List<string>(split);
                    newSplit.Remove("$");
                    c = codeSwitch(newSplit.ToArray(), newSplit[0]);
                    if(inloop){
                        try
                        {
                            if(loopObj is null) { 
                                throw new CompileException("<internal> [Compile.cs/codeSwitch] loopObj is null!");
                            }
                            loopObj.runs.Add(c);
                            dontadd = true;
                            return loopObj;
                        }
                        catch (System.Exception e)
                        {
                            Program.log("SOMETHING WENT WRONG!");
                            Program.log("action: " + action);
                            Program.log("split: " + string.Join(",", split));
                            Program.log(c.instruction);
                            Program.log(string.Join(",",newSplit));
                            Program.log(e.ToString());
                            break;
                        }
                        
                    }else{
                        Console.WriteLine(c.instruction);
                        throw new CompileException("Not in loop!");
                    }
                    
                case "%":
                    inloop = false;
                    loopObj = new code_block();
                    dontadd = true;
                break;
                default:
                    throw new CompileException("Invalid instruction!", new InvalidTokenException(action, new ButtersException("in line : " + (line + 2).ToString())));
            }
            return c;
        }

        public void generate(string[] file){
            SECTION? currentSection = null;
            META meta = new META();
            List<VAR> vars = new List<VAR>();
            for (int i = 0; i < file.Length; i++)
            {
                string[] split = file[i].Split(" "); 
                string action = split[0];
                if(action == "#section"){
                    switch (split[1])
                    {
                        case "META":
                            currentSection = SECTION.META;
                        break;
                        case "STATIC":
                            currentSection = SECTION._STATIC;
                        break;
                        case "DYNAMIC":
                            currentSection = SECTION._DYNAMIC;
                        break;
                        case "DEFINE":
                            currentSection = SECTION.DEFINE;
                        break;
                        case "CODE":
                            currentSection = SECTION.CODE;
                        break;
                        default:
                            currentSection = null;
                        break;
                    }
                }
                switch(currentSection){
                    case SECTION.META:
                        switch (action)
                        {
                            case "*author":
                                List<string> a = new List<string>(split);
                                a.Remove("*author");
                                meta.author = string.Join(" ", a);
                            break;
                            case "*version":
                                List<string> v = new List<string>(split);
                                v.Remove("*version");
                                meta.version = string.Join(" ", v);
                            break;
                            case "*project":
                                List<string> p = new List<string>(split);
                                p.Remove("*project");
                                meta.project = string.Join(" ", p);
                            break;
                            case "*license":
                                List<string> l = new List<string>(split);
                                l.Remove("*license");
                                meta.license = string.Join(" ", l);
                            break;
                        }
                    break;
                    case SECTION.DEFINE:
                        switch (action)
                        {
                            case "#section":
                                continue;
                            case "float":
                                VAR f = new VAR();
                                f.name = split[1];
                                f.type = VAR.VarType.FLOAT;
                                f.value = float.Parse(split[2]);
                                vars.Add(f);
                            break;
                            case "int":
                                VAR i_ = new VAR();
                                i_.name = split[1];
                                i_.type = VAR.VarType.INT;
                                i_.value = int.Parse(split[2]);
                                vars.Add(i_);
                            break;
                            case "string":
                            case "var":
                                VAR s = new VAR();
                                s.name = split[1];
                                s.type = VAR.VarType.STRING;
                                List<string> sL = new List<string>(split);
                                sL.Remove("string");
                                sL.Remove("var");
                                sL.Remove(s.name);
                                s.value = string.Join(" ", sL);
                                vars.Add(s);
                            break;
                            case "bool":
                                VAR b = new VAR();
                                b.name = split[1];
                                b.type = VAR.VarType.STRING;
                                b.value = split[2] == "true" ? true : false;
                                vars.Add(b);
                            break;
                            case "void":
                            case ".float":
                            case ".int":
                            case ".string":
                                throw new NotImplementedException();
                            default:
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine(split[0] + " is not a valid variable type!");
                                Console.ForegroundColor = ConsoleColor.White;
                                System.Environment.Exit(-1);
                            break;
                        }
                    break;
                    case SECTION.CODE:
                        if (action == "#section")
                        {
                            continue;
                        }
                        code_block c = codeSwitch(split, action, i);
                        
                        if(!dontadd){
                            code.Add(c);
                        }

                    break;
                    case (SECTION)1:
                        bool it = split[1] == "DYNAMIC";
                        if(action == "#section"){
                            continue;
                        }
                        if(action != "import"){
                            throw new CompileException("invalid action for section: " + action);
                        }
                        if(!File.Exists(split[1] + ".btrs")){
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("make sure DYNAMIC imports are existing files ending with .btrs in the filename but just the filename without extension in code");
                            Console.WriteLine("make sure STATIC imports are existing files ending with .dll in the filename but just the filename without extension in code");
                            Console.ForegroundColor = ConsoleColor.White;
                            throw new CompileException("Import does not exist!");
                        }
                        Console.WriteLine(it);
                        Console.WriteLine(File.Exists(split[1] + (it ? ".btrs" : ".dll")));
                    break;
                }
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Program.log("<META> " + JsonSerializer.Serialize<META>(meta));
            Program.log("<DEFINE> " + JsonSerializer.Serialize<List<VAR>>(vars));
            Program.log("<CODE> " + JsonSerializer.Serialize<List<code_block>>(code));
            Console.ForegroundColor = ConsoleColor.White;

            var options = new JsonSerializerOptions { IgnoreNullValues = true };
            string metaJson = JsonSerializer.Serialize(meta, options);
            string varsJson = JsonSerializer.Serialize(vars, options);
            string codeJson = JsonSerializer.Serialize(code, options);

            write(metaJson, varsJson, codeJson);
            _latestMETA = meta;
         }

        public void write(string META_JSON, string DEFINE_JSON, string CODE_JSON){
            string project = JsonSerializer.Deserialize<META>(META_JSON).project;
            Console.ForegroundColor = ConsoleColor.Green;
            Program.log("[Compile.cs/write] Writing to " + project + ".bcomp");
            string generated = "{\n";
            Program.log("[Compile.cs/write] Writing meta");
            generated += "  \"meta\": " + META_JSON + ",\n";    // write meta
            Program.log("[Compile.cs/write] Writing static");
            generated += "  \"_static\": " + "[]" + ",\n";    // write static imports
            Program.log("[Compile.cs/write] Writing dynamic");
            generated += "  \"_dynamic\": " + "[]" + ",\n";    // write dynamic imports
            Program.log("[Compile.cs/write] Writing define");
            generated += "  \"define\": " + DEFINE_JSON + ",\n";    // write variables
            Program.log("[Compile.cs/write] Writing code");
            generated += "  \"code\": " + CODE_JSON + "\n";    // write variables
            generated += "}";
            File.WriteAllText(project+".bcomp", generated);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public string[] readFile(string file){
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Program.log("[Compile.cs/comp] reading file " + file);
            Console.ForegroundColor = ConsoleColor.White;
            string[] f = File.ReadAllLines(file);
            
            for (int i = 0; i < f.Length; i++)
            {
                if(String.Concat(f[i].Where(c => !Char.IsWhiteSpace(c))) == ""){
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Program.log("[Compile.cs/comp] removing empty line");
                    Console.ForegroundColor = ConsoleColor.White;
                    var temp = new List<string>(f);
                    temp.RemoveAt(i);
                    f = temp.ToArray(); 
                }
            }

            for (int i = 0; i < f.Length; i++)
            {
                if(f[i][0] == '-' && f[i][1] == '-'){
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Program.log("[Compile.cs/comp] removing comment");
                    Console.ForegroundColor = ConsoleColor.White;
                    var temp = new List<string>(f);
                    temp.RemoveAt(i);
                    f = temp.ToArray(); 
                }
            }

            return f;
        }
    }
}