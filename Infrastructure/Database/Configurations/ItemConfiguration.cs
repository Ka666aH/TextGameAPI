using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TextGame.Domain.Entities.GameObjects.Items;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Chestplates;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Armors.Helms;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Swords;
using TextGame.Domain.Entities.GameObjects.Items.Equipments.Weapons.Wands;
using TextGame.Domain.Entities.GameObjects.Items.Heals;
using TextGame.Domain.Entities.GameObjects.Items.Other;

namespace TextGame.Infrastructure.Database.Configurations
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.ToTable("Items");

            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Description).IsRequired().HasMaxLength(250);

            builder.HasDiscriminator<string>("ItemType")
                .HasValue<BagOfCoins>("BagOfCoins")
                .HasValue<Key>("Key")
                .HasValue<Map>("Map")
                .HasValue<Chest>("Chest")

                .HasValue<Bandage>("Bandage")
                .HasValue<RegenPotion>("RegenPotion")
                .HasValue<PowerPotion>("PowerPotion")
                .HasValue<RandomPotion>("RandomPotion")

                .HasValue<Fists>("Fists")
                .HasValue<RustSword>("RustSword")
                .HasValue<IronSword>("IronSword")
                .HasValue<SilverSword>("SilverSword")
                .HasValue<GlassSword>("GlassSword")
                .HasValue<MagicWand>("MagicWand")
                .HasValue<RandomWand>("RandomWand")

                .HasValue<WoodenBucket>("WoodenBucket")
                .HasValue<LeatherHelm>("LeatherHelm")
                .HasValue<IronHelm>("IronHelm")
                .HasValue<LeatherVest>("LeatherVest")
                .HasValue<IronCuirass>("IronCuirass");
        }
    }
}