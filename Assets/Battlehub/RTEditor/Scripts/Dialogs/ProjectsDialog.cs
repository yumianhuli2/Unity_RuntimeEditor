﻿using Battlehub.RTCommon;
using Battlehub.RTSaveLoad2.Interface;
using Battlehub.UIControls;

using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class ProjectsDialog : MonoBehaviour
    {
        [SerializeField]
        private VirtualizingTreeView m_treeView;
        
        [SerializeField]
        private Sprite ProjectIcon;

        private PopupWindow m_parentPopup;

        private IProject m_project;

        [SerializeField]
        private Button m_btnNew;

        [SerializeField]
        private Button m_btnDelete;

 
        private void Start()
        {
            m_parentPopup = GetComponentInParent<PopupWindow>();
            if (m_parentPopup != null)
            {
                m_parentPopup.OK.AddListener(OnOK);
            }


            if (m_treeView == null)
            {
                Debug.LogError("m_builtInTreeView == null");
                return;
            }

            m_treeView.ItemDataBinding += OnItemDataBinding;
            m_treeView.ItemDoubleClick += OnItemDoubleClick;
            m_treeView.CanDrag = false;
            m_treeView.CanEdit = false;
            m_treeView.CanUnselectAll = false;

            m_project = IOC.Resolve<IProject>();
           
            IRTE editor = IOC.Resolve<IRTE>();

            m_parentPopup.IsContentLoaded = false;
            editor.IsBusy = true;

            m_project.ListProjects((error, projectInfo) =>
            {
                m_parentPopup.IsContentLoaded = true;
                editor.IsBusy = false;

                m_treeView.Items = projectInfo;
                if(projectInfo != null && projectInfo.Length > 0)
                {
                    m_treeView.SelectedIndex = 0;
                }

                if (m_btnNew != null)
                {
                    m_btnNew.onClick.AddListener(OnCreateProjectClick);
                }

                if (m_btnDelete != null)
                {
                    m_btnDelete.onClick.AddListener(OnDestroyProjectClick);
                }
            });
        }

        private void OnDestroy()
        {
            if (m_parentPopup != null)
            {
                m_parentPopup.OK.RemoveListener(OnOK);
            }

            if (m_treeView != null)
            {
                m_treeView.ItemDataBinding -= OnItemDataBinding;
                m_treeView.ItemDoubleClick -= OnItemDoubleClick;
            }

            if (m_btnNew != null)
            {
                m_btnNew.onClick.RemoveListener(OnCreateProjectClick);
            }

            if (m_btnDelete != null)
            {
                m_btnDelete.onClick.RemoveListener(OnDestroyProjectClick);
            }
        }

        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            string item = e.Item as string;
            if (item != null)
            {
                Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
                text.text = item;

                Image image = e.ItemPresenter.GetComponentInChildren<Image>(true);
                image.sprite = ProjectIcon;
                image.gameObject.SetActive(true);

                e.HasChildren = false;
            }
        }

        private void OnItemDoubleClick(object sender, ItemArgs e)
        {
            m_parentPopup.Close(true);
        }

        private void OnOK(PopupWindowArgs args)
        {
            if (m_treeView.SelectedItem == null)
            {
                args.Cancel = true;
                return;
            }
        }

        private void OnCreateProjectClick()
        {

        }

        private void OnDestroyProjectClick()
        {
            PopupWindow.Show("Delete Project", "Delete selected project?", "Delete", args =>
            {
                ProjectInfo selectedProject = (ProjectInfo)m_treeView.SelectedItem;
                m_project.DeleteProject(selectedProject.Name, (error, deletedProject) =>
                {

                });
            },
            "Cancel");

        }
    }
}