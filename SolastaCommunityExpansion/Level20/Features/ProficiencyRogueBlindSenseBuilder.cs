﻿using SolastaCommunityExpansion.Builders;
using SolastaModApi.Extensions;
using static SolastaModApi.DatabaseHelper.FeatureDefinitionSenses;

namespace SolastaCommunityExpansion.Level20.Features
{
    internal class ProficiencyRogueBlindSenseBuilder : DefinitionBuilder<FeatureDefinitionSense>
    {
        private const string ProficiencyRogueBlindSenseName = "ZSProficiencyRogueBlindSense";
        private const string ProficiencyRogueBlindSensedGuid = "30c27691f42f4705985c638d38fadc21";

        protected ProficiencyRogueBlindSenseBuilder(string name, string guid) : base(SenseBlindSight2, name, guid)
        {
            Definition.GuiPresentation.Title = "Feature/&ProficiencyRogueBlindSenseTitle";
            Definition.GuiPresentation.Description = "Feature/&ProficiencyRogueBlindSenseDescription";
            Definition.GuiPresentation.SetHidden(false);
        }

        private static FeatureDefinitionSense CreateAndAddToDB(string name, string guid)
        {
            return new ProficiencyRogueBlindSenseBuilder(name, guid).AddToDB();
        }

        internal static readonly FeatureDefinitionSense ProficiencyRogueBlindSense
            = CreateAndAddToDB(ProficiencyRogueBlindSenseName, ProficiencyRogueBlindSensedGuid);
    }
}