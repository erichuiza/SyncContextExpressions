using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Tridion.ContentManager.CoreService.Client;

namespace Tridion.Extensions.ContextExpressions
{
    class TargetGroupSynchronizer : IDisposable
    {
        private const string _syncDescriptionMarker = "Synchronized Context Expression";
        private readonly string _syncRootFolderWebDavUrl;
        private static readonly ApplicationData _synchronizerContextData = new ApplicationData { ApplicationId = "ce:TargetGroupSynchronizer", Data = new byte[0] };
        private static readonly ReadOptions _defaultReadOptions = new ReadOptions();
        private readonly SessionAwareCoreServiceClient _coreServiceClient;
        private readonly List<TargetGroupData> _synchronizedTargetGroups;

        public TargetGroupSynchronizer(string syncRootFolderWebDavUrl)
        {
            _syncRootFolderWebDavUrl = syncRootFolderWebDavUrl;

            _coreServiceClient = new SessionAwareCoreServiceClient("netTcp_2013");
            _coreServiceClient.SetSessionContextData(_synchronizerContextData);

            OrganizationalItemItemsFilterData targetGroupFilter = new OrganizationalItemItemsFilterData
            {
                ItemTypes = new[] { ItemType.TargetGroup },
                Recursive = true, // Allow synced Target Groups to be organized in sub-Folders.
                BaseColumns = ListBaseColumns.IdAndTitle,
                IncludeDescriptionColumn = true
            };
            _synchronizedTargetGroups = _coreServiceClient.GetList(syncRootFolderWebDavUrl, targetGroupFilter).Cast<TargetGroupData>().ToList();
        }

        public void Dispose()
        {
            if (_coreServiceClient.State == CommunicationState.Opened)
            {
                _coreServiceClient.Close();
            }
            else
            {
                _coreServiceClient.Abort();
            }
        }

        public void SyncTargetGroup(string name, string version, string contextExpression, string description = null)
        {
            TargetGroupData existingTargetGroup = _synchronizedTargetGroups.FirstOrDefault(tg => tg.Title == name);
            if (existingTargetGroup == null)
            {
                CreateTargetGroup(name, version, contextExpression, description);
            }
            else
            {
                UpdateTargetGroup(existingTargetGroup, version, contextExpression, description);
            }
        }

        private static string GetSyncDescription(string name, string version)
        {
            return string.Format(_syncDescriptionMarker + " '{0}' version {1}", name, version);
        }

        private void CreateTargetGroup(string name, string version, string contextExpression,string description)
        {
            TargetGroupData targetGroupData = new TargetGroupData
            {
                LocationInfo = new LocationInfo
                {
                    OrganizationalItem = new LinkToOrganizationalItemData { IdRef = _syncRootFolderWebDavUrl }
                },
                Title = name,
                Description = description ?? GetSyncDescription(name, version)
            };
            targetGroupData = (TargetGroupData)_coreServiceClient.Create(targetGroupData, _defaultReadOptions);

            TargetGroupExtensionData targetGroupExtensionData = new TargetGroupExtensionData
            {
                ContextExpression = contextExpression,
                SyncLabel = version
            };
            ApplicationDataAdapter appDataAdapter = new ApplicationDataAdapter(TargetGroupExtensionData.ApplicationId, targetGroupExtensionData);
            _coreServiceClient.SaveApplicationData(targetGroupData.Id, new[] { appDataAdapter.ApplicationData });

            _synchronizedTargetGroups.Add(targetGroupData);
        }

        private void UpdateTargetGroup(TargetGroupData existingTargetGroup, string version, string contextExpression, string description)
        {
            // Always update the Target Group itself to update its revision date
            // Update the Target Group Description if it isn't customized yet.
            TargetGroupData deltaData = new TargetGroupData { Id = existingTargetGroup.Id };
            if (existingTargetGroup.Description.StartsWith(_syncDescriptionMarker))
            {
                deltaData.Description = description ?? GetSyncDescription(existingTargetGroup.Title, version);
            }
            _coreServiceClient.Update(deltaData, null);

            TargetGroupExtensionData targetGroupExtensionData = new TargetGroupExtensionData
            {
                ContextExpression = contextExpression,
                SyncLabel = version
            };
            ApplicationDataAdapter appDataAdapter = new ApplicationDataAdapter(TargetGroupExtensionData.ApplicationId, targetGroupExtensionData);
            _coreServiceClient.SaveApplicationData(existingTargetGroup.Id, new[] { appDataAdapter.ApplicationData });
        }
    }
}
