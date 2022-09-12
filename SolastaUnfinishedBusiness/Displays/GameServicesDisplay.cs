﻿using System;
using System.Collections.Generic;
using System.Linq;
using SolastaUnfinishedBusiness.Api.ModKit;
using SolastaUnfinishedBusiness.DataViewer;
using UnityEngine;

namespace SolastaUnfinishedBusiness.Displays;

internal static class GameServicesDisplay
{
    private static readonly Dictionary<string, Func<object>> TargetList = new()
    {
        { "None", null },
        { "IGamingPlatformService", ServiceRepository.GetService<IGamingPlatformService> },
        { "ICharacterBuildingService", ServiceRepository.GetService<ICharacterBuildingService> },
        { "ICharacterPoolService", ServiceRepository.GetService<ICharacterPoolService> },
        { "IGameCampaignService", ServiceRepository.GetService<IGameCampaignService> },
        { "IGameFactionService", ServiceRepository.GetService<IGameFactionService> },
        { "IHeroBuildingCommandService", ServiceRepository.GetService<IHeroBuildingCommandService> },
        { "INetworkingService", ServiceRepository.GetService<INetworkingService> },
        { "IGameLocationActionService", ServiceRepository.GetService<IGameLocationActionService> },
        { "IGameLocationAudioService", ServiceRepository.GetService<IGameLocationAudioService> },
        { "IGameLocationBanterService", ServiceRepository.GetService<IGameLocationBanterService> },
        { "IGameLocationBattleService", ServiceRepository.GetService<IGameLocationBattleService> },
        { "IGameLocationCharacterService", ServiceRepository.GetService<IGameLocationCharacterService> },
        { "IGameLocationEnvironmentService", ServiceRepository.GetService<IGameLocationEnvironmentService> },
        { "IGameLocationGadgetService", ServiceRepository.GetService<IGameLocationGadgetService> },
        { "IGameLocationItemService", ServiceRepository.GetService<IGameLocationItemService> },
        { "IGameLocationMapService", ServiceRepository.GetService<IGameLocationMapService> },
        { "IGameLocationPathfindingService", ServiceRepository.GetService<IGameLocationPathfindingService> },
        { "IGameLocationPositioningService", ServiceRepository.GetService<IGameLocationPositioningService> },
        { "IGameLocationSelectionService", ServiceRepository.GetService<IGameLocationSelectionService> },
        { "IGameLocationService", ServiceRepository.GetService<IGameLocationService> },
        { "IGameLocationTargetingService", ServiceRepository.GetService<IGameLocationTargetingService> },
        { "IGameLocationTimelineService", ServiceRepository.GetService<IGameLocationTimelineService> },
        { "IGameLocationVisibilityService", ServiceRepository.GetService<IGameLocationVisibilityService> },
        { "IGameLoreService", ServiceRepository.GetService<IGameLoreService> },
        { "IGameQuestService", ServiceRepository.GetService<IGameQuestService> },
        { "IGameRestingService", ServiceRepository.GetService<IGameRestingService> },
        { "IGameScavengerService", ServiceRepository.GetService<IGameScavengerService> },
        { "IGameSerializationService", ServiceRepository.GetService<IGameSerializationService> },
        { "IGameService", ServiceRepository.GetService<IGameService> },
        { "IGameSettingsService", ServiceRepository.GetService<IGameSettingsService> },
        { "IGameVariableService", ServiceRepository.GetService<IGameVariableService> }
    };

    private static readonly string[] TargetNames = TargetList.Keys.ToArray();

    private static ReflectionTreeView TreeView { get; } = new();

    private static void ResetTree()
    {
        var getTarget = TargetList[TargetNames[Main.Settings.SelectedRawDataType]];

        if (getTarget == null)
        {
            TreeView.Clear();
        }
        else
        {
            TreeView.SetRoot(getTarget());
        }
    }

    public static void DisplayGameServices()
    {
        try
        {
            if (TreeView == null)
            {
                ResetTree();
            }

            // target selection
            GUIHelper.SelectionGrid(ref Main.Settings.SelectedRawDataType, TargetNames, 8, ResetTree);

            // tree view
            if (Main.Settings.SelectedRawDataType == 0)
            {
                return;
            }

            GUILayout.Space(10f);

            TreeView?.OnGUI();
        }
        catch
        {
            Main.Settings.SelectedRawDataType = 0;
            TreeView?.Clear();
        }
    }
}
