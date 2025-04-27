using System;
using System.Text;
using ElectricalProgressive.Interface;
using ElectricalProgressive.Utils;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace ElectricalProgressive.Content.Item.Weapon;

public class EWeapon : Vintagestory.API.Common.Item,IEnergyStorageItem
{
    int consume;
    int maxcapacity;
    int fireCost;

    private double lastUpdateTime = 0;
    private const double interval = 5000; //интервал обновления меча

   

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);

        CollectibleBehaviorAnimationAuthoritative collectibleBehaviorAnimationAuthoritative = GetCollectibleBehavior<CollectibleBehaviorAnimationAuthoritative>(withInheritance: true);
        if (collectibleBehaviorAnimationAuthoritative == null)
        {
            api.World.Logger.Warning("Spear {0} uses ItemSpear class, but lacks required AnimationAuthoritative behavior. I'll take the freedom to add this behavior, but please fix json item type.", Code);
            collectibleBehaviorAnimationAuthoritative = new CollectibleBehaviorAnimationAuthoritative(this);
            collectibleBehaviorAnimationAuthoritative.OnLoaded(api);
            CollectibleBehaviors = CollectibleBehaviors.Append(collectibleBehaviorAnimationAuthoritative);
        }

        collectibleBehaviorAnimationAuthoritative.OnBeginHitEntity += EWeapon_OnBeginHitEntity;
        

        consume = MyMiniLib.GetAttributeInt(this, "consume", 20);
        maxcapacity = MyMiniLib.GetAttributeInt(this, "maxcapacity", 20000);
        fireCost = MyMiniLib.GetAttributeInt(this, "fireCost", 0);

    }





    

    /// <summary>
    /// Обновление меча в руке
    /// </summary>
    /// <param name="world"></param>
    /// <param name="stack"></param>
    public override void OnHeldIdle(ItemSlot slot, EntityAgent byEntity)
    {

        double currentTime = api.World.ElapsedMilliseconds;
        if (slot.Itemstack.Item.Variant["type"] == "hot" && currentTime - lastUpdateTime >= interval)
        {
            
            DamageItem(api.World, byEntity, slot);
            lastUpdateTime = currentTime;

            if (slot.Itemstack.Attributes.GetInt("durability")<=1) //тушим
            {
                var newItem = api.World.GetItem(new AssetLocation("electricalprogressiveequipment", "static-saber-common"));
                var newStack = new ItemStack(newItem);
                if (slot.Itemstack.Attributes != null)
                {
                    newStack.Attributes = slot.Itemstack.Attributes.Clone();
                }

                slot.Itemstack = newStack;
                
            }

            slot.MarkDirty();

        }


    }

    public override void OnHeldAttackStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandHandling handling)
    {
        int energy = slot.Itemstack.Attributes.GetInt("electricalprogressive:energy");
        if (energy < consume)
        {
            handling = EnumHandHandling.PreventDefault;
            return;
        }


        base.OnHeldAttackStart(slot, byEntity, blockSel, entitySel, ref handling);


    }


    /// <summary>
    /// Пока на земле
    /// </summary>
    /// <param name="entityItem"></param>
    public override void OnGroundIdle(EntityItem entityItem)
    {
        base.OnGroundIdle(entityItem);
        double currentTime = api.World.ElapsedMilliseconds;
        if (entityItem.Itemstack.Item.Variant["type"] == "hot" &&  currentTime - lastUpdateTime >= interval)
        {
                        
            var slot = new DummySlot();
            slot.Itemstack = entityItem.Itemstack; //связываем их
            DamageItem(api.World, null, slot);
            slot.MarkDirty();
            entityItem.Itemstack = slot.Itemstack;            

            lastUpdateTime = currentTime;

            if (entityItem.Itemstack.Attributes.GetInt("durability") <= 1) //тушим
            {
                var newItem = api.World.GetItem(new AssetLocation("electricalprogressiveequipment", "static-saber-common"));
                var newStack = new ItemStack(newItem);
                if (entityItem.Itemstack.Attributes != null)
                {
                    newStack.Attributes = entityItem.Itemstack.Attributes.Clone();
                }

                entityItem.Itemstack = newStack; // Обновляем предмет сущности

            }
                        
        }
       
    }

    /// <summary>
    /// Попал по противнику
    /// </summary>
    /// <param name="byEntity"></param>
    /// <param name="handling"></param>
    private void EWeapon_OnBeginHitEntity(EntityAgent byEntity, ref EnumHandling handling)
    {
        if (byEntity.World.Side == EnumAppSide.Client)
        {
            return;
        }

        EntitySelection entitySelection = (byEntity as EntityPlayer)?.EntitySelection;
        //меч горит и противник живой
        if (entitySelection != null && entitySelection.Entity.Alive && this.Variant["type"] == "hot")
        {
            entitySelection.Entity.IsOnFire = true;
        }
    }


    public override string GetHeldTpUseAnimation(ItemSlot activeHotbarSlot, Entity byEntity)
    {
        return null;
    }



    public override bool OnHeldAttackCancel(float secondsPassed, ItemSlot slot, EntityAgent byEntity,
        BlockSelection blockSelection, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
    {
        return false;
    }


    public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
    {
        return secondsUsed < 2.0F || secondsUsed > 2.1F;
    }


    /// <summary>
    /// Левая кнопка мыши зажата
    /// </summary>
    /// <param name="secondsPassed"></param>
    /// <param name="slot"></param>
    /// <param name="byEntity"></param>
    /// <param name="blockSelection"></param>
    /// <param name="entitySel"></param>
    /// <returns></returns>
    public override bool OnHeldAttackStep(float secondsPassed, ItemSlot slot, EntityAgent byEntity,
       BlockSelection blockSelection, EntitySelection entitySel)
    {

        int energy = slot.Itemstack.Attributes.GetInt("electricalprogressive:energy");
        secondsPassed *= 1.25f;

        float backwards = -Math.Min(0.35f, 2 * secondsPassed);
        float stab = Math.Min(1.2f, 20 * Math.Max(0, secondsPassed - 0.35f));

        if (byEntity.World.Side == EnumAppSide.Client)
        {
            IClientWorldAccessor world = byEntity.World as IClientWorldAccessor;


            if (stab > 1.15f && byEntity.Attributes.GetInt("didattack") == 0 && energy >= consume)
            {
                world.TryAttackEntity(entitySel);
                byEntity.Attributes.SetInt("didattack", 1);
                world.AddCameraShake(0.25f);
            }
        }



        return secondsPassed < 1.2f;

    }



    public override void DamageItem(IWorldAccessor world, Entity byEntity, ItemSlot itemslot, int amount = 1)
    {
        int energy = itemslot.Itemstack.Attributes.GetInt("electricalprogressive:energy");
        if (energy >= consume * amount)
        {
            energy -= consume * amount;
            itemslot.Itemstack.Item.SetDurability(itemslot.Itemstack, Math.Max(1, energy / consume));
            itemslot.Itemstack.Attributes.SetInt("electricalprogressive:energy", energy);
        }
        else
        {
            itemslot.Itemstack.Item.SetDurability(itemslot.Itemstack, 1);
        }


    }


    public override void OnHeldAttackStop(float secondsPassed, ItemSlot slot, EntityAgent byEntity,
        BlockSelection blockSelection, EntitySelection entitySel)
    {

    }




    /// <summary>
    /// Удерживаем правую кнопку мыши
    /// </summary>
    /// <param name="secondsUsed"></param>
    /// <param name="slot"></param>
    /// <param name="byEntity"></param>
    /// <param name="blockSel"></param>
    /// <param name="entitySel"></param>
    public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
    {

        //время удерживания по сути
        if (secondsUsed < 2.0f || secondsUsed > 2.1F)
        {
            return;
        }

        byEntity.StopAnimation("helditemready");

        ItemStack EW = slot.Itemstack;

        //зажигаем
        int energy = EW.Attributes.GetInt("electricalprogressive:energy");
        if (EW.Item.Variant["type"] != "hot")
        {
            //хватает заряда?            
            if (energy >= fireCost)
            {
                energy -= fireCost;

                EW.Item.SetDurability(EW, Math.Max(1, energy / consume));
                EW.Attributes.SetInt("electricalprogressive:energy", energy);
                slot.MarkDirty();
            }
            else
            {
                return;
            }



            var newItem = api.World.GetItem(new AssetLocation("electricalprogressiveequipment", "static-saber-hot"));
            var newStack = new ItemStack(newItem);
            if (EW.Attributes != null)
            {
                newStack.Attributes = EW.Attributes.Clone();
            }

            slot.Itemstack = newStack;
            slot.MarkDirty();

            if (byEntity.World.Side == EnumAppSide.Client)
            {
                byEntity.World.PlaySoundAt(new AssetLocation("game:sounds/effect/swoosh.ogg"), byEntity, null, false);
            }
        }
        else //тушим
        {
            var newItem = api.World.GetItem(new AssetLocation("electricalprogressiveequipment", "static-saber-common"));
            var newStack = new ItemStack(newItem);
            if (EW.Attributes != null)
            {
                newStack.Attributes = EW.Attributes.Clone();
            }

            slot.Itemstack = newStack;
            slot.MarkDirty();
        }



    }



    /// <summary>
    /// Нажал правую кнопку мыши
    /// </summary>
    /// <param name="itemslot"></param>
    /// <param name="byEntity"></param>
    /// <param name="blockSel"></param>
    /// <param name="entitySel"></param>
    /// <param name="firstEvent"></param>
    /// <param name="handling"></param>
    public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
    {
        base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handling);
        if (handling != EnumHandHandling.PreventDefault)
        {
            handling = EnumHandHandling.PreventDefault;
            byEntity.StartAnimation("helditemready");
        }

    }


    /// <summary>
    /// Отпустил правую кнопку мыши
    /// </summary>
    /// <param name="secondsUsed"></param>
    /// <param name="slot"></param>
    /// <param name="byEntity"></param>
    /// <param name="blockSel"></param>
    /// <param name="entitySel"></param>
    /// <param name="cancelReason"></param>
    /// <returns></returns>
    public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason)
    {
        return true;
    }


    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        dsc.AppendLine(inSlot.Itemstack.Attributes.GetInt("electricalprogressive:energy") + "/" + maxcapacity + " " + Lang.Get("W"));
    }
    
    public int receiveEnergy(ItemStack itemstack, int maxReceive)
    {
        int received = Math.Min(maxcapacity - itemstack.Attributes.GetInt("electricalprogressive:energy"), maxReceive);
        itemstack.Attributes.SetInt("electricalprogressive:energy", itemstack.Attributes.GetInt("electricalprogressive:energy") + received);
        int durab = Math.Max(1, itemstack.Attributes.GetInt("electricalprogressive:energy") / consume);
        itemstack.Attributes.SetInt("durability", durab);
        return received;
    }


    public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
    {
        return new WorldInteraction[] {
                new WorldInteraction()
                {
                    ActionLangCode = "saber_right",
                    MouseButton = EnumMouseButton.Right,
                }
            }.Append(base.GetHeldInteractionHelp(inSlot));
    }
}