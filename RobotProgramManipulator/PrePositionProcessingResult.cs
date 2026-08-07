using System.Collections.Generic;
using System.Text;

namespace ISRA.RobotProgramManipulator
{
    internal sealed class PrePositionProcessingResult
    {
        private readonly List<string> _messages = new List<string>();

        public int CreatedCount { get; private set; }

        public int SkippedCount { get; private set; }

        public int ErrorCount { get; private set; }

        public IList<string> Messages
        {
            get { return _messages.AsReadOnly(); }
        }

        public void AddCreated()
        {
            CreatedCount++;
        }

        public void AddSkipped(string message)
        {
            SkippedCount++;
            _messages.Add(message);
        }

        public void AddError(string message)
        {
            ErrorCount++;
            _messages.Add(message);
        }

        public string CreateSummary()
        {
            var summary = new StringBuilder();
            summary.AppendLine("Created: " + CreatedCount);
            summary.AppendLine("Skipped: " + SkippedCount);
            summary.AppendLine("Errors: " + ErrorCount);

            if (_messages.Count > 0)
            {
                summary.AppendLine();
                summary.AppendLine("Details:");
                int messageCount = System.Math.Min(_messages.Count, 15);
                for (int index = 0; index < messageCount; index++)
                {
                    summary.AppendLine("- " + _messages[index]);
                }

                if (_messages.Count > messageCount)
                {
                    summary.AppendLine("- " + (_messages.Count - messageCount) + " more messages not shown.");
                }
            }

            return summary.ToString().TrimEnd();
        }
    }
}
