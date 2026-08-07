namespace ISRA.RobotProgramManipulator
{
    internal enum RobotProgramType
    {
        Measurement,
        Service
    }

    internal sealed class PrePositionOptions
    {
        public PrePositionOptions(
            RobotProgramType programType,
            bool includeExistingPrePositions,
            double[] axisOffsetsDegrees,
            bool addComment,
            string commentDeclarationBefore,
            string commentDeclarationAfter)
        {
            ProgramType = programType;
            IncludeExistingPrePositions = includeExistingPrePositions;
            AxisOffsetsDegrees = axisOffsetsDegrees;
            AddComment = addComment;
            CommentDeclarationBefore = commentDeclarationBefore ?? string.Empty;
            CommentDeclarationAfter = commentDeclarationAfter ?? string.Empty;
        }

        public RobotProgramType ProgramType { get; private set; }

        public bool IncludeExistingPrePositions { get; private set; }

        public double[] AxisOffsetsDegrees { get; private set; }

        public bool AddComment { get; private set; }

        public string CommentDeclarationBefore { get; private set; }

        public string CommentDeclarationAfter { get; private set; }
    }
}
