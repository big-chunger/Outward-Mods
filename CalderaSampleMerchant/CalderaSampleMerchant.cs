using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using SideLoader;

namespace CalderaSampleMerchant
{
    [BepInPlugin(ID, NAME, VERSION)]
    public class CalderaSampleMerchant : BaseUnityPlugin
    {
        const string ID = "com.apernicus.calderasamplemerchant";
        const string NAME = "Caldera Sample Merchant";
        const string VERSION = "0.1.0";

        public static CalderaSampleMerchant Instance;
        public static ManualLogSource MyLogger = BepInEx.Logging.Logger.CreateLogSource(NAME);

        List<int> molepigs = new List<int>()
        {
            6000420,
            6000421,
            6000422,
            6000423,
            6000424,
            6000425
        };

        List<int> plants = new List<int>()
        {
            6000380,
            6000381,
            6000382,
            6000383,
            6000384,
            6000385
        };

        List<int> ores = new List<int>()
        {
            6000340,
            6000341,
            6000342,
            6000343,
            6000344,
            6000345
        };

        internal void Awake()
        {
            Instance = this;

            AlterItems();
            CreateDropTable();
            CreateItemSource();
        }

        void AlterItems()
        {
            foreach (var item in molepigs)
            {
                ChangeSamplePrice(item);
            }

            foreach (var item in plants)
            {
                ChangeSamplePrice(item);
            }

            foreach (var item in ores)
            {
                ChangeSamplePrice(item);
            }
        }

        void CreateDropTable()
        {
            var table = new SL_DropTable
            {
                UID = "apernicus.sampledroptable",

                RandomTables = new List<SL_RandomDropGenerator>
                {
                    RandomDropGenerator(molepigs),
                    RandomDropGenerator(plants),
                    RandomDropGenerator(ores)
                }
            };

            table.ApplyTemplate();
        }

        void CreateItemSource()
        {
            var source = new SL_DropTableAddition
            {
                //Original Name
                IdentifierName = "apernicus.samplemerchantitemsource",
                //Merchant UID - Sorobor Caravan in New Sirocco
                SelectorTargets = new List<string>
                {
                    "-MSrkT502k63y3CV2j98TQ"
                },
                //UID of Drop Table created above
                DropTableUIDsToAdd = new List<string>
                {
                    "apernicus.sampledroptable"
                }
            };

            source.ApplyTemplate();
        }

        SL_RandomDropGenerator RandomDropGenerator(List<int> sampleIDList)
        {
            var dropGenerator = new SL_RandomDropGenerator
            {
                MaxNumberOfDrops = 6,
                NoDrop_DiceValue = 4,
                Drops = DropChances(sampleIDList)
            };

            return dropGenerator;
        }

        List<SL_ItemDropChance> DropChances(List<int> sampleIDList)
        {
            var drops = new List<SL_ItemDropChance>();

            foreach (var ID in sampleIDList)
            {
                var drop = new SL_ItemDropChance
                {
                    DroppedItemID = ID,
                    DiceValue = 1
                };

                drops.Add(drop);
            }

            return drops;
        }

        void ChangeSamplePrice(int itemID)
        {
            var item = new SL_Item
            {
                Target_ItemID = itemID,
                New_ItemID = -1,
                StatsHolder = new SL_ItemStats
                {
                    BaseValue = 400,
                    RawWeight = 6,
                    MaxDurability = -1
                }
            };

            item.ApplyTemplate();
        }
    }
}
