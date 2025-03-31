using System;
using System.Text.Json;
using System.IO;
using System.Diagnostics;
using System.Data;
using System.Text.RegularExpressions;

namespace butters{
    class guide{
        public META meta { get; set; }
        public string[] _static { get; set; }
        public string[] _dynamic { get; set; }
        public VAR[] define { get; set; }
        public code_block[] code { get; set; }
    }
    class runtime{
        public META meta;
        public guide code;
        public List<VAR> vars;
        public runtime(string bcompPath){
            this.meta = new META();
            string file = File.ReadAllText(bcompPath);
            this.code = JsonSerializer.Deserialize<guide>(file);
            Program.log(JsonSerializer.Serialize<guide>(code));
            this.vars = new List<VAR>();
        }
        
        public void run(Stopwatch stopwatch){
            // start timer if timed is on
            if(Program.timed == true){
                stopwatch.Start();
            }

            // import dynamic files
            foreach (string import in code._dynamic)
            {
                Program.log("[runtime.cs/run] found dynamic import: " + import);
                importBTRS(import);
            }

            // Import static files
            foreach (string import in code._static)
            {
                Program.log("[runtime.cs/run] found static import: " + import);
                importDLL(import);
            }

            // create variables
            foreach (VAR variable in code.define)
            {
                Program.log("[runtime.cs/run] found definition for variable: " + variable.name);
                vars.Add(variable);
            }

            runcode(code.code);

            if(Program.timed == true){
                stopwatch.Stop();
                Console.WriteLine($"finished in {stopwatch.ElapsedMilliseconds} milliseconds.");
            }
        }


        static List<string> pins = new List<string>();
        static code_block[] top_level_code;
        static bool istop = true;

        private void runcode(code_block[] code, bool warping = false)
        {
            if(istop){
                istop = false;
                top_level_code = code;
            }
            bool skipnextWarp = false;
            string str;
            string[] strs;
            string expression;
            foreach (code_block block in code)
            {
                if(warping){
                    System.Threading.Thread.Sleep(Program.warp_delay);
                }
                switch(block.instruction){
                    case "out":
                        str = getVar(block.var);
                        Console.WriteLine(str);
                    break;
                    case "input":
                        str = getVar(block.var);
                        Console.WriteLine(str);
                        setVar(Console.ReadLine(), block.output);
                    break;
                    case "redef":
                        if(isVar(block.value)){
                            setVar(block.var, getVar(block.value));
                        }else{
                            strs = block.value.Split(" ");
                            for (int i = 0; i < strs.Length; i++)
                            {
                                if(strs[i].Substring(0, 1) == "_"){
                                    strs[i] = getVar(strs[i].Replace("_", ""));
                                }
                                if(strs[i] == "-~-" || strs[i] == "-SPACE-"){
                                    strs[i] = " ";
                                }
                            }
                            str = string.Join(" ", strs);
                            bool isValidExpression = Regex.IsMatch(str, @"^\s*\d+(\s*[-+\/*]\s*\d+)*\s*$");
                            if(isValidExpression){
                                var dataTable = new DataTable();
                                setVar(dataTable.Compute(str, "").ToString(), block.var);
                            }else{
                                setVar(str, block.var);
                            }
                        }
                    break;
                    case "while":
                        strs = block.condition.Split(" ");
                        for (int i = 0; i < strs.Length; i++)
                        {
                            if(strs[i].Substring(0, 1) == "_"){
                                strs[i] = getVar(strs[i].Replace("_", ""));
                            }
                        }
                        expression = string.Join("", strs);
                        while (EvaluateBooleanExpression(expression))
                        {
                            runcode(block.runs.ToArray());
                            strs = block.condition.Split(" ");
                            for (int i = 0; i < strs.Length; i++)
                            {
                                if(strs[i].Substring(0, 1) == "_"){
                                    strs[i] = getVar(strs[i].Replace("_", ""));
                                }
                            }
                           expression = string.Join("", strs);
                        }
                    break;
                    case "pin":
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Program.log("[runtime.cs/runcode] found warp point " + block.value);
                        Console.ForegroundColor = ConsoleColor.White;
                        pins.Add(block.value);
                        string pinstring = "";
                        foreach (string pin in pins)
                        {
                            pinstring += pin + ", ";
                        }
                        Program.log("[runtime.cs/runcode] all current pins: [" +  pinstring + "]");
                    break;
                    case "jump":
                        if(skipnextWarp){
                            continue;
                        }
                        List<code_block> temp_code = new List<code_block>();
                        foreach(code_block b in top_level_code){
                            if(b.instruction == "pin" && b.value == block.value) { 
                                Program.log("[runtime.cs/runcode] Found pin " + b.value);
                                temp_code = b.runs;
                                break;
                            }
                        }

                        if (temp_code.Count == 0)
                        {
                            throw new InvalidWarpException("warp is empty or does not exist!", new InvalidTokenException(block.value));
                        }

                        runcode(temp_code.ToArray(), true);
                    break;
                    case "return":
                        skipnextWarp = true;
                        return;
                    case "if":
                        strs = block.condition.Split(" ");
                        for (int i = 0; i < strs.Length; i++)
                        {
                            if(strs[i].Substring(0, 1) == "_"){
                                string varname = strs[i].Substring(1, strs[i].Length - 1);
                                if(getRawVar(varname).type == VAR.VarType.STRING){
                                    strs[i] = "'" + getVar(varname) + "'";
                                }else{
                                    strs[i] = getVar(varname);
                                }

                            }
                        }
                        expression = string.Join(" ", strs);
                        Program.log("[runtime.cs/runcode] evaluating: " + expression);
                        if (EvaluateBooleanExpression(expression))
                        {
                            runcode(block.runs.ToArray());
                            strs = block.condition.Split(" ");
                        }
                    break;
                    case "noop":
                        Program.log("[runtime.cs/runcode] Encountered no-op instruction.");
                        break;
                    default:
                        throw new InvalidTokenException(block.instruction, new ButtersException("invalid token"));
                }
            }
        }

        private string getVar(string name){
            if(!isVar(name)){
                throw new InvalidVariableException(name);
            }
            return vars.FirstOrDefault(x => x.name == name).value.ToString();
        }

        private VAR getRawVar(string name){
            if(!isVar(name)){
                throw new InvalidVariableException(name);
            }
            return vars.FirstOrDefault(var => var.name == name);
        }

        private bool isVar(string name){
            return vars.Any(x => x.name == name);
        }

        private void setVar(dynamic val, string name){
            if(!isVar(name)){
                throw new InvalidVariableException(name);
            }
            vars.FirstOrDefault(x => x.name == name).value = val;
            return;
        }

        static bool EvaluateBooleanExpression(string expression)
        {
            Program.log("[runtime.cs/EvaluateBooleanExpression] evaluating: " + expression);

            var dataTable = new System.Data.DataTable();

            var result = dataTable.Compute(expression, "");

            Program.log("[runtime.cs/EvaluateBooleanExpression] evalution result: " + result.ToString());

            return result is bool && (bool)result == true;
        }

        private void importDLL(string import)
        {
            // get variables from file
            throw new NotImplementedException();
        }

        private void importBTRS(string import)
        {
            // Run the file and extract variables
            throw new NotImplementedException();
        }
    }
}