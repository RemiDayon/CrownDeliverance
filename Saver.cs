using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Saver : MonoBehaviour
{
    static FileStream file = null;
    static BinaryReader br = null;
    static BinaryWriter bw = null;

    public static string dollyTrack = "DollyTrack";

    public static void Save()
    {
        file = new FileStream(Application.dataPath + "/Resource/World.dat", FileMode.Create);
        bw = new BinaryWriter(file);

        SaveScene();
        bw.Write(dollyTrack);
        SaveImportant();

        bw.Close();
        file.Close();

        file = new FileStream(Application.dataPath + "/Resource/Player.dat", FileMode.Create);
        bw = new BinaryWriter(file);

        bw.Write(PlayerManager.main.FocusedCharacter.name);
        SaveCharacter(PlayerManager.main.Fairy);
        SaveCharacter(PlayerManager.main.Tank);
        SaveCharacter(PlayerManager.main.Rogue);

        bw.Close();
        file.Close();
    }

    public static void DestroySave()
    {
        file = new FileStream(Application.dataPath + "/Resource/Player.dat", FileMode.Create);
        file.Close();
        file = new FileStream(Application.dataPath + "/Resource/World.dat", FileMode.Create);
        file.Close();
    }

    public void SaveParameter()
    {
        file = new FileStream(Application.dataPath + "/Resource/Param.dat", FileMode.Create);
        bw = new BinaryWriter(file);
        
        bw.Write(MusicsManager.volumeMain);
        bw.Write(MusicsManager.volumeFX);
        bw.Write(MusicsManager.volumeMusic);
        bw.Write(BlitGodRays.DrawGodRays);

        bw.Close();
        file.Close();
    }

    public static void LoadParameter()
    {
        file = new FileStream(Application.dataPath + "/Resource/Param.dat", FileMode.OpenOrCreate);
        br = new BinaryReader(file);

        if (file.Position != file.Length)
        {
            MusicsManager.volumeMain = br.ReadSingle();
            MusicsManager.volumeFX = br.ReadSingle();
            MusicsManager.volumeMusic = br.ReadSingle();
            BlitGodRays.DrawGodRays = br.ReadBoolean();
        }
       
        br.Close();
        file.Close();
    }

    private static void SaveScene()
    {
        bw.Write(SceneManager.GetActiveScene().name);
    }

    private static void SaveCharacter(Character _char)
    {
        bw.Write(_char.transform.position.x);
        bw.Write(_char.transform.position.y);
        bw.Write(_char.transform.position.z);        
        bw.Write(_char.transform.rotation.x);        
        bw.Write(_char.transform.rotation.y);        
        bw.Write(_char.transform.rotation.z);
        bw.Write(_char.Level);
        bw.Write(_char.GetExp);
        bw.Write(_char.ConstitutionBase);
        bw.Write(_char.PVBase);
        bw.Write(_char.PV);
        bw.Write(_char.StrengthBase);
        bw.Write(_char.DexterityBase);
        bw.Write(_char.IntelligenceBase);
        bw.Write(_char.MobilityBase);
        bw.Write(_char.DodgeBase);
        bw.Write(_char.CriticalChanceBase);
        bw.Write(_char.CriticalDamageBase);
        bw.Write(_char.Gold);        

        bw.Write(_char.availableSkillTreePoints);
        Skill[] skillTree = _char.gameObject.GetComponent<SkillTreeReader>().skillTree;
        int unlocked = 0;
        int unlocked_1 = 0;
        for (int x = 0; x < skillTree.Length; ++x)
        {
            if (x < 32)
            {
                unlocked |= ((skillTree[x].unlocked) ? 1 : 0) << x;
            }
            else
            {
                unlocked_1 |= ((skillTree[x].unlocked) ? 1 : 0) << x;
            }
        }        
        bw.Write(unlocked);
        bw.Write(unlocked_1);

        bw.Write(_char.RightHand != null);
        if (_char.RightHand)
        {
            SaveItem(_char.RightHand);
        }

        bw.Write(_char.LeftHand != null);
        if (_char.LeftHand)
        {
            SaveItem(_char.LeftHand);
        }

        bw.Write(_char.Body != null);
        if (_char.Body)
        {
            SaveItem(_char.Body);
        }

        SaveInventory(_char);
    }

    private static void SaveInventory(Character _char)
    {
        for (int x = 0; x < _char.Inventory.Length; ++x)
        {
            if (_char.Inventory[x] != null && _char.Inventory[x].Item != null && _char.Inventory[x].Nb > 0)
            {
                bw.Write(x);
                bw.Write(_char.Inventory[x].Nb);
                SaveItem(_char.Inventory[x].Item);
            }
        }
        bw.Write(-1);
    }

    private static void SaveItem(Pickable _item)
    {
        bw.Write(_item.Name);
        bw.Write((int)_item.Rarity);

        ItemEffect[] effects = _item.GetComponentsInChildren<ItemEffect>();

        if (_item as Consumable != null)
        {
            bw.Write((char)0);
            Consumable cons = (Consumable)_item;
            bw.Write(cons.UtilisationNB);
        }
        else if (_item as Weapon != null)
        {
            bw.Write((char)1);
            Weapon weap = (Weapon)_item;
            bw.Write(weap.PowerBase);
            bw.Write(weap.LevelRequired);
            bw.Write((int)weap.ClassRequired);
        }
        else if (_item as Armor != null)
        {
            bw.Write((char)2);
            Armor arm = (Armor)_item;
            bw.Write(arm.PowerBase);
            bw.Write(arm.LevelRequired);
            bw.Write((int)arm.ClassRequired);
        }
        else if (_item as Shield != null)
        {
            bw.Write((char)3);
            Shield shi = (Shield)_item;
            bw.Write(shi.PowerBase);
            bw.Write(shi.LevelRequired);
            bw.Write((int)shi.ClassRequired);
        }

        bw.Write(effects.Length);
        foreach (ItemEffect effect in effects)
        {
            SaveItemEffect(effect);
        }
    }

    private static void SaveImportant()
    {
        foreach(string str in Important.importantList)
        {
            bw.Write(str);
        }
    }

    public static string LoadScene()
    {
        file = new FileStream(Application.dataPath + "/Resource/World.dat", FileMode.OpenOrCreate);
        br = new BinaryReader(file);
        string sceneName = "Blockout Temp";

        if (file.Position == file.Length)
        {
            br.Close();
            file.Close();
            return sceneName;
        }

        sceneName = br.ReadString();
        dollyTrack = br.ReadString();

        br.Close();
        file.Close();

        return sceneName;
    }

    public static bool LoadCharacters()
    {
        file = new FileStream(Application.dataPath + "/Resource/Player.dat", FileMode.OpenOrCreate);
        br = new BinaryReader(file);

        if (file.Position == file.Length)
        {
            br.Close();
            file.Close();
            return false;
        }

        string main = br.ReadString();
        switch(main)
        {
            case "Hymelia":
                PlayerManager.main.FocusedCharacter = PlayerManager.main.PlayerFairy;
                break;

            case "Akayel":
                PlayerManager.main.FocusedCharacter = PlayerManager.main.PlayerRogue;
                break;

            case "Ryveck":
                PlayerManager.main.FocusedCharacter = PlayerManager.main.PlayerTank;
                break;
        }

        GameObject itemStorage;
        if ((itemStorage = GameObject.Find("ItemStorage")) == null)
        {
            itemStorage = new GameObject("ItemStorage");
            DontDestroyOnLoad(itemStorage);
        }

        LoadCharacter(PlayerManager.main.PlayerFairy, itemStorage);
        LoadCharacter(PlayerManager.main.PlayerTank, itemStorage);
        LoadCharacter(PlayerManager.main.PlayerRogue, itemStorage);
        
        br.Close();
        file.Close();

        return true;
    }

    private static void LoadCharacter(Character _character, GameObject _itemStorage)
    {
        _character.transform.position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());           
        _character.transform.rotation = new Quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),0f);
        _character.Level = br.ReadInt32();
        _character.SetXp = br.ReadInt32();
        _character.ConstitutionBase = br.ReadInt32();
        _character.PVBase = br.ReadInt32();
        _character.PV = br.ReadInt32();
        _character.StrengthBase = br.ReadInt32();
        _character.DexterityBase = br.ReadInt32();
        _character.IntelligenceBase = br.ReadInt32();
        _character.MobilityBase = br.ReadInt32();
        _character.DodgeBase = br.ReadInt32();
        _character.CriticalChanceBase = br.ReadInt32();
        _character.CriticalDamageBase = br.ReadInt32();
        _character.Gold = br.ReadInt32(); 

        _character.availableSkillTreePoints = br.ReadInt32();
        
        Skill[] skills = _character.gameObject.GetComponent<SkillTreeReader>().skillTree;
        int unlocked = br.ReadInt32();
        int unlocked_1 = br.ReadInt32();
        for (int x = 0; x < skills.Length; ++x)
        {
            if (x < 32)
            {
                skills[x].unlocked = (unlocked >> x & 1) == 1;
            }
            else
            {
                skills[x].unlocked = (unlocked_1 >> x & 1) == 1;
            }
        }

        if (br.ReadBoolean())
        {
            _character.RightHand = (Equipable)LoadItem(_itemStorage);
        }

        if (br.ReadBoolean())
        {
            _character.LeftHand = (Equipable)LoadItem(_itemStorage);
        }

        if (br.ReadBoolean())
        {
            _character.Body = (Equipable)LoadItem(_itemStorage);
        }

        LoadInventory(_character, _itemStorage);
    }

    public static void LoadInventory(Character _character, GameObject _itemStorage)
    {
        int index;
        while ((index = br.ReadInt32()) != -1)
        {
            if (_character.Inventory[index] == null)
            {
                _character.Inventory[index] = new ItemInventory();
            }
            _character.Inventory[index].Nb = br.ReadInt32();
            _character.Inventory[index].Item = LoadItem(_itemStorage);
        }
    }

    public static Pickable LoadItem(GameObject _storage)
    {
        string name = br.ReadString();
        int rarity = br.ReadInt32();
        char type = br.ReadChar();

        GameObject item;//= new GameObject(name);
        Pickable pick = null;

        switch(type)
        {
            case (char)0:
                item = Instantiate(ObjManager.main.objectTypeConsommable[0]);
                Consumable cons = item.GetComponent<Consumable>();
                cons.UtilisationNB = br.ReadInt32();
                cons.Rarity = (RARITY)rarity;
                pick = cons;
                break;

            case (char)1:
                {

                    float powerBase = br.ReadSingle();
                    int lvl = br.ReadInt32();

                    switch((CLASSES)br.ReadInt32())
                    {
                        case CLASSES.ASSASSIN:
                        item = Instantiate(ObjManager.main.objectTypeWeaponRogue[rarity]);
                            break;
                        case CLASSES.TANK:
                        item = Instantiate(ObjManager.main.objectTypeWeaponTank[rarity]);
                            break;
                        case CLASSES.WIZARD:
                        item = Instantiate(ObjManager.main.objectTypeWeaponFee[rarity]);
                            break;
                        default:
                            item = new GameObject();
                            break;
                    }

                    Weapon weap = item.GetComponent<Weapon>();
                    weap.PowerBase = powerBase;
                    weap.LevelRequired = lvl;
                    pick = weap;
                }
                break;

            case (char)2:
                {
                    float powerBase = br.ReadSingle();
                    int LevelRequired = br.ReadInt32();
                    switch((CLASSES)br.ReadInt32())
                    {
                        case CLASSES.ASSASSIN:
                            item = Instantiate(ObjManager.main.objectTypeArmorRogue[rarity]);
                            break;
                        case CLASSES.TANK:
                            item = Instantiate(ObjManager.main.objectTypeArmorTank[rarity]);
                            break;
                        case CLASSES.WIZARD:
                            item = Instantiate(ObjManager.main.objectTypeArmorFee[rarity]);
                            break;
                        default:
                            item = new GameObject();
                            break;
                    }
                    Armor arm = item.GetComponent<Armor>();
                    arm.PowerBase = powerBase;
                    arm.LevelRequired = LevelRequired;
                    pick = arm;
                }
                break;

            case (char)3:
                item = Instantiate(ObjManager.main.objectTypeWeaponShield[rarity]);
                Shield shi = item.GetComponent<Shield>();
                shi.PowerBase = br.ReadSingle();
                shi.LevelRequired = br.ReadInt32();
                shi.ClassRequired = (CLASSES)br.ReadInt32();
                pick = shi;
                break;
            default:
                item = new GameObject();
                break;
        }

        int effectNb = br.ReadInt32();
        for (int x = 0; x < effectNb; ++x)
        {
            GameObject effect = new GameObject();
            LoadItemEffect(effect);
            effect.transform.parent = item.transform;
        }
        item.name = name;
        item.transform.parent = _storage.transform;
        return pick;
    }

    public static void LoadImportant()
    {
        file = new FileStream(Application.dataPath + "/Resource/World.dat", FileMode.OpenOrCreate);
        br = new BinaryReader(file);

        if (file.Position == file.Length)
        {
            br.Close();
            file.Close();
            return;
        }

        br.ReadString();
        br.ReadString();

        string imp;
        while (file.Position != file.Length)
        {
            imp = br.ReadString();
            Important.importantList.Add(imp);
        }

        br.Close();
        file.Close();
    }

    private static void SaveItemEffect(ItemEffect _effect)
    {
        if (_effect as ItemEffectAddArmor)
        {
            bw.Write((char)0);

            ItemEffectAddArmor effect = (ItemEffectAddArmor)_effect;
            bw.Write(effect.Armor);
        }
        else if (_effect as ItemEffectArmor)
        {
            bw.Write((char)1);
            ItemEffectArmor effect = (ItemEffectArmor)_effect;
            bw.Write(effect.ArmorBonus);
        }
        else if (_effect as ItemEffectConstitution)
        {
            bw.Write((char)2);
            ItemEffectConstitution effect = (ItemEffectConstitution)_effect;
            bw.Write(effect.ConsBonus);
        }
        else if (_effect as ItemEffectDexterity)
        {
            bw.Write((char)3);
            ItemEffectDexterity effect = (ItemEffectDexterity)_effect;
            bw.Write(effect.DextBonus);
        }
        else if (_effect as ItemEffectHeal)
        {
            bw.Write((char)4);
            ItemEffectHeal effect = (ItemEffectHeal)_effect;
            bw.Write(effect.Heal);
        }
        else if (_effect as ItemEffectIntelligence)
        {
            bw.Write((char)5);
            ItemEffectIntelligence effect = (ItemEffectIntelligence)_effect;
            bw.Write(effect.IntelligenceBonus);
        }
        else if (_effect as ItemEffectStrength)
        {
            bw.Write((char)6);
            ItemEffectStrength effect = (ItemEffectStrength)_effect;
            bw.Write(effect.StrengthBonus);
        }
        else if (_effect as ItemEffectModiferBaseStat)
        {
            bw.Write((char)7);
            ItemEffectModiferBaseStat effect = (ItemEffectModiferBaseStat)_effect;
            bw.Write(effect.StatsUp.armor);
            bw.Write(effect.StatsUp.constitution);
            bw.Write(effect.StatsUp.dexterity);
            bw.Write(effect.StatsUp.intelligence);
            bw.Write(effect.StatsUp.strength);
        }
        else if (_effect as ItemEffectModifierSecondaryStat)
        {
            bw.Write((char)8);
            ItemEffectModifierSecondaryStat effect = (ItemEffectModifierSecondaryStat)_effect;
            bw.Write(effect.StatsUp.criticalChanceBonus);
            bw.Write(effect.StatsUp.criticalDamageBonus);
            bw.Write(effect.StatsUp.dodgeBonus);
            bw.Write(effect.StatsUp.mobilityBonus);
            bw.Write(effect.StatsUp.totalPaBonus);
        }
        else if (_effect as ItemEffectAlterationAutoBurning)
        {
            bw.Write((char)9);
        }
        else if (_effect as ItemEffectAlterationAutoFrost)
        {
            bw.Write((char)10);
        }
        else if (_effect as ItemEffectAlterationAutoGalvanised)
        {
            bw.Write((char)11);
        }
        else
        {
            Debug.Log("Item currently not supported by the save : " + _effect.ToString());
            Debug.Log(" please add it to the script 'Saver : SaveItemEffect - LoadItemEffect' (or at least ask to the genius of the group to do it)");
            bw.Write(char.MaxValue);
            bw.Write(_effect.ToString());
        }
    }

    private static void LoadItemEffect(GameObject _Item)
    {
        switch(br.ReadChar())
        {
            case (char)0:
                _Item.AddComponent<ItemEffectAddArmor>().Armor = br.ReadInt32();
                break;

            case (char)1:
                _Item.AddComponent<ItemEffectArmor>().ArmorBonus = br.ReadInt32();
                break;

            case (char)2:
                _Item.AddComponent<ItemEffectConstitution>().ConsBonus = br.ReadInt32();
                break;

            case (char)3:
                _Item.AddComponent<ItemEffectDexterity>().DextBonus = br.ReadInt32();
                break;

            case (char)4:
                _Item.AddComponent<ItemEffectHeal>().Heal = br.ReadInt32();
                break;

            case (char)5:
                _Item.AddComponent<ItemEffectIntelligence>().IntelligenceBonus = br.ReadInt32();
                break;

            case (char)6:
                _Item.AddComponent<ItemEffectStrength>().StrengthBonus = br.ReadInt32();
                break;

            case (char)7:
                ItemEffectModiferBaseStat itEffBasStat =_Item.AddComponent<ItemEffectModiferBaseStat>();
                BaseStats bs = new BaseStats();
                bs.armor =br.ReadInt32();
                bs.constitution =br.ReadInt32();
                bs.dexterity =br.ReadInt32();
                bs.intelligence =br.ReadInt32();
                bs.strength = br.ReadInt32();
                itEffBasStat.StatsUp = bs;
                break;
            case (char)8:
                ItemEffectModifierSecondaryStat itEffSecondaryStat = _Item.AddComponent<ItemEffectModifierSecondaryStat>();
                SecondaryStats SecStat = new SecondaryStats();
                SecStat.criticalChanceBonus= br.ReadInt32();
                SecStat.criticalDamageBonus = br.ReadInt32();
                SecStat.dodgeBonus = br.ReadInt32();
                SecStat.mobilityBonus = br.ReadInt32();
                SecStat.totalPaBonus = br.ReadInt32();
                itEffSecondaryStat.StatsUp = SecStat;
                break;
            case (char)9:
                {
                    int index = 0;
                    for (int i = 0; i < ObjManager.main.itemEffectAlterationWeapon.Count; i++)
                    {
                        if (ObjManager.main.itemEffectAlterationWeapon[i].GetComponent<ItemEffectAlterationAutoBurning>() != null)
                        {
                            index = i;
                            break;
                        }
                    }
                    Instantiate(ObjManager.main.itemEffectAlterationWeapon[index], _Item.transform);
                }
                break;
            case (char)10:
                {
                    int index = 0;
                    for (int i = 0; i < ObjManager.main.itemEffectAlterationWeapon.Count; i++)
                    {
                        if (ObjManager.main.itemEffectAlterationWeapon[i].GetComponent<ItemEffectAlterationAutoFrost>() != null)
                        {
                            index = i;
                            break;
                        }
                    }
                    Instantiate(ObjManager.main.itemEffectAlterationWeapon[index], _Item.transform);
                }
                break;
            case (char)11:
                {
                    int index = 0;
                    for (int i = 0; i < ObjManager.main.itemEffectAlterationWeapon.Count; i++)
                    {
                        if (ObjManager.main.itemEffectAlterationWeapon[i].GetComponent<ItemEffectAlterationAutoGalvanised>() != null)
                        {
                            index = i;
                            break;
                        }
                    }
                    Instantiate(ObjManager.main.itemEffectAlterationWeapon[index], _Item.transform);
                }
                break;
            case char.MaxValue:
                Debug.Log("Item currently not supported by the save : " + br.ReadString() + " please add it to the script 'Saver : SaveItemEffect - LoadItemEffect'");
                Debug.Log("I mean seriously guy, if you can't save it why would you be able to load it ? ga... ");
                break;
        }
    }
}
