using Mono.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WordleBot
{
    public class Options
    {
        public string DictionaryName { get; private set; }
        public int VocabularySize { get; private set; } = int.MaxValue;
        public bool UseRandomSolution { get; private set; }
        public bool GuessCandidatesOnly { get; private set; }
        public bool CalculateStatistics { get; private set; }

        public static Options Parse(string[] args)
        {
            Options options = new();
            OptionSet optionSet = new()
            {
                { "d|dictionary=", "use named dictionary", value => options.DictionaryName = value },
                { "v|vocabulary=", "use vocabulary size hint", (int value) => options.VocabularySize = value },
                { "r|random-solution", "run with a random solution", _ => options.UseRandomSolution = true },
                { "c|candidates-only", "guess matching candidates only", _ => options.GuessCandidatesOnly = true },
                { "s|calc-stats", "calculate stats over all solutions", _ => options.CalculateStatistics = true },
            };

            var usage = new StringBuilder($"Usage:\n");
            optionSet.WriteOptionDescriptions(new StringWriter(usage));

            IList<string> invalidArgs;
            try
            {
                invalidArgs = optionSet.Parse(args);
            }
            catch (OptionException e)
            {
                throw new Exception($"Invalid option: {e.Message}\n{usage}");
            }

            if (invalidArgs.Any())
            {
                throw new ArgumentException($"Unknown option: {String.Join(" ", invalidArgs)}\n{usage}");
            }

            return options;
        }
    }
}
