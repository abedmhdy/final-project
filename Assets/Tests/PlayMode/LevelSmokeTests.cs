using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// Loads every level and checks that the weapon & loot setup is wired up:
// the player starts with a weapon, the HUD has all its references, the
// wave rewards exist and enemies carry loot tables. Game scripts live in
// Assembly-CSharp, which an asmdef can't reference, so components are
// looked up by type name and read through reflection. Any error or
// exception logged during a test makes it fail.
public class LevelSmokeTests
{
    private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static Component Find(string typeName) =>
        UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(c => c != null && c.GetType().Name == typeName);

    private static object Get(object target, string member)
    {
        Type type = target.GetType();
        FieldInfo field = type.GetField(member, Fields);
        if (field != null) return field.GetValue(target);
        PropertyInfo property = type.GetProperty(member, Fields);
        Assert.IsNotNull(property, $"{type.Name} has no member '{member}'");
        return property.GetValue(target);
    }

    private static bool IsNull(object value) => value == null || (value is UnityEngine.Object o && o == null);

    private static IEnumerator LoadLevel(string scene)
    {
        SceneManager.LoadScene(scene);
        yield return null;
        yield return null;
        Time.timeScale = 1f;
    }

    [UnityTest]
    public IEnumerator Level_IsWiredUp([Values("Level1", "Level2", "Level3")] string scene)
    {
        yield return LoadLevel(scene);

#if UNITY_EDITOR
        int missing = SceneManager.GetActiveScene().GetRootGameObjects()
            .Sum(root => root.GetComponentsInChildren<Transform>(true)
                .Sum(t => UnityEditor.GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)));
        Assert.AreEqual(0, missing, "missing scripts in scene");
#endif

        GameObject player = GameObject.FindWithTag("Player");
        Assert.IsNotNull(player, "no Player-tagged object");
        Component combat = player.GetComponent("PlayerCombat");
        Assert.IsNotNull(combat, "Player has no PlayerCombat");
        Assert.IsFalse(IsNull(Get(combat, "startingWeapon")), "PlayerCombat.startingWeapon not set");
        Assert.IsFalse(IsNull(Get(combat, "weaponVisual")), "PlayerCombat.weaponVisual not set");
        object weapon = Get(combat, "CurrentWeapon");
        Assert.IsFalse(IsNull(weapon), "no weapon equipped after Start");
        Debug.Log($"[Smoke] {scene}: starting weapon = {Get(weapon, "weaponName")}");

        Component hud = Find("HUDController");
        Assert.IsNotNull(hud, "no HUDController");
        foreach (FieldInfo field in hud.GetType().GetFields(Fields)
                     .Where(f => typeof(UnityEngine.Object).IsAssignableFrom(f.FieldType)))
        {
            Assert.IsFalse(IsNull(field.GetValue(hud)), $"HUDController.{field.Name} not assigned");
        }
        Component weaponText = (Component)Get(hud, "weaponText");
        StringAssert.Contains((string)Get(weapon, "weaponName"), (string)Get(weaponText, "text"), "HUD weapon text not updated");

        Component spawner = Find("WaveSpawner");
        Assert.IsNotNull(spawner, "no WaveSpawner");
        Assert.IsFalse(IsNull(Get(spawner, "exitDoor")), "WaveSpawner.exitDoor not set");
        var waves = ((Array)Get(spawner, "waves")).Cast<object>().ToList();
        Assert.IsNotEmpty(waves);
        foreach (object wave in waves)
        foreach (object info in (Array)Get(wave, "enemiesToSpawn"))
        {
            var prefab = (GameObject)Get(info, "enemyPrefab");
            Assert.IsFalse(IsNull(prefab), "wave enemy prefab missing");
            Assert.IsFalse(IsNull(Get(info, "spawnPoint")), "wave spawn point missing");
            Component dropper = prefab.GetComponent("LootDropper");
            Assert.IsNotNull(dropper, $"{prefab.name} has no LootDropper");
            Assert.IsFalse(IsNull(Get(dropper, "lootTable")), $"{prefab.name} LootDropper has no table");
        }

        var rewards = waves.Where(w => !IsNull(Get(w, "clearReward"))).ToList();
        if (scene == "Level1")
        {
            Assert.IsEmpty(rewards, "Level1 should have no wave weapon reward");
        }
        else
        {
            Assert.IsNotEmpty(rewards, $"{scene} has no wave reward");
            foreach (object wave in rewards)
            {
                Assert.IsFalse(IsNull(Get(wave, "rewardSpawnPoint")), "reward spawn point missing");
                Component pickup = ((GameObject)Get(wave, "clearReward")).GetComponent("WeaponPickup");
                Assert.IsNotNull(pickup, "reward is not a WeaponPickup");
                object rewardWeapon = Get(pickup, "Weapon");
                Assert.IsFalse(IsNull(rewardWeapon), "reward WeaponPickup has no WeaponData");
                Assert.Greater((int)Get(rewardWeapon, "tier"), (int)Get(weapon, "tier"), "reward is not an upgrade");
                Debug.Log($"[Smoke] {scene}: wave reward = {Get(rewardWeapon, "weaponName")}");
            }
        }
    }

    // Plays a level through without input: kills every wave, checks the
    // reward appears, walks the player onto it and checks it gets equipped.
    [UnityTest]
    public IEnumerator Level_PlaysThrough([Values("Level1", "Level2", "Level3")] string scene)
    {
        yield return LoadLevel(scene);

        GameObject player = GameObject.FindWithTag("Player");
        Component combat = player.GetComponent("PlayerCombat");
        object startWeapon = Get(combat, "CurrentWeapon");
        Component spawner = Find("WaveSpawner");
        int waveCount = ((Array)Get(spawner, "waves")).Length;
        var aliveEnemies = (IList)Get(spawner, "aliveEnemies");

        bool cleared = false;
        var rewards = new List<GameObject>();
        Type spawnerType = spawner.GetType();
        Action onCleared = () => cleared = true;
        Action<GameObject> onReward = r => rewards.Add(r);
        EventInfo clearedEvent = spawnerType.GetEvent("OnAllWavesCleared");
        EventInfo rewardEvent = spawnerType.GetEvent("OnRewardSpawned");
        clearedEvent.AddEventHandler(null, onCleared);
        rewardEvent.AddEventHandler(null, onReward);

        try
        {
            float deadline = Time.time + 20f * waveCount + 10f;
            while (!cleared && Time.time < deadline)
            {
                foreach (Component enemy in aliveEnemies.Cast<Component>().ToList())
                {
                    if (enemy != null) enemy.GetType().GetMethod("TakeDamage").Invoke(enemy, new object[] { 9999 });
                }
                // Park the player far away so enemies can't kill it meanwhile.
                player.transform.position = new Vector3(-100f, -100f, 0f);
                yield return new WaitForSeconds(0.25f);
            }
            Assert.IsTrue(cleared, $"{scene}: waves never cleared");

            if (scene == "Level1")
            {
                Assert.IsEmpty(rewards);
                yield break;
            }

            Assert.IsNotEmpty(rewards, $"{scene}: no reward spawned");
            GameObject reward = rewards.Last(r => r != null);
            object rewardWeapon = Get(reward.GetComponent("WeaponPickup"), "Weapon");
            player.transform.position = reward.transform.position;
            yield return new WaitForSeconds(0.5f);

            object equipped = Get(combat, "CurrentWeapon");
            Assert.AreNotSame(startWeapon, equipped, $"{scene}: reward was not equipped");
            Assert.AreSame(rewardWeapon, equipped);
            Assert.IsTrue(reward == null, "picked-up reward should be destroyed");
            Debug.Log($"[Smoke] {scene}: equipped {Get(equipped, "weaponName")} from wave reward");
        }
        finally
        {
            clearedEvent.RemoveEventHandler(null, onCleared);
            rewardEvent.RemoveEventHandler(null, onReward);
        }
    }
}
