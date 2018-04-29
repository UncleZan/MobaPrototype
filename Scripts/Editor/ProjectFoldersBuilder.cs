using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
 
public class buildProjectFolders : ScriptableWizard
{
    private string SFGUID;
    List<List<string>> subfolders = new List<List<string>>() { new List<string> { "Classes", "Editor", "Interfaces", "States" }, new List<string> { "Music", "SFX" }, new List<string> { "Levels", "Other" }};
    List<string> fwSub = new List<string>() { "Scripts", "Audio", "Scenes" };
    List<string> folders = new List<string>() { "3rd-Party", "Animations", "Artwork", "Audio", "Materials", "Models", "Plugins", "Prefabs", "Resources", "Textures", "Sandbox", "Scenes", "Scripts", "Sprites" };
    List<string> sceneContainers = new List<string>() { "Management", "GUI", "Camera", "Lights", "World", "_Dynamic" };
    public bool pfChecked = false, shChecked = false;
    [MenuItem("Edit/Set Game Structure...")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard("Set Game Structure", typeof(buildProjectFolders));
    }

    //Called when the window first appears
    void OnEnable()
    {

    }
    
    //Create button click
    void OnWizardCreate()
    {
        //create all the folders required in a project
        //primary and sub folders
        #region ProjectFolders
        if (pfChecked)
        {
            Debug.Log("FOLDERS CREATED");
            foreach (string folder in folders)
            {
                if (!AssetDatabase.IsValidFolder("Assets/" + folder))
                    AssetDatabase.CreateFolder("Assets", folder);
            }

            AssetDatabase.Refresh();

            for (int i = 0; i < subfolders.Count; i++)
            {
                foreach (string sf in subfolders[i])
                {
                    if (!AssetDatabase.IsValidFolder("Assets/" + fwSub[i] + "/" + sf))
                        AssetDatabase.CreateFolder("Assets/" + fwSub[i], sf);
                }
            }
        }
        #endregion

        #region SceneHierarchy
        if (shChecked)
        {
            Debug.Log("SCENE CREATED");
            GameObject gameObject;
            for (int i = 0; i < sceneContainers.Count; i++)
            {
                if (GameObject.Find(sceneContainers[i]) == null)
                    gameObject = new GameObject(sceneContainers[i]);
                else
                    gameObject = new GameObject();

                if (sceneContainers[i].Equals("World"))
                {
                    GameObject subGameObject = new GameObject("Terrain");
                    subGameObject.transform.SetParent(gameObject.transform);
                    subGameObject = new GameObject("Props");
                    subGameObject.transform.SetParent(gameObject.transform);
                    subGameObject = new GameObject("Characters");
                    subGameObject.transform.SetParent(gameObject.transform);
                }
            }
        }        
        #endregion
    }

    //Runs whenever something changes in the editor window...
    void OnWizardUpdate()
    {

    }

    void removeNamespceFolders()
    {
        if (subfolders.Count > 0) subfolders.Clear();
    }
}


