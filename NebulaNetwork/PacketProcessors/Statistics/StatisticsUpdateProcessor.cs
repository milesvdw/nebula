﻿using NebulaAPI;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Statistics;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Statistics
{
    [RegisterPacketProcessor]
    internal class StatisticsUpdateProcessor : PacketProcessor<StatisticUpdateDataPacket>
    {
        public override void ProcessPacket(StatisticUpdateDataPacket packet, NebulaConnection conn)
        {
            StatisticalSnapShot snapshot;
            using (Multiplayer.Session.Statistics.IsIncomingRequest.On())
            {
                using (BinaryUtils.Reader reader = new BinaryUtils.Reader(packet.StatisticsBinaryData))
                {
                    bool itemChanged = false;
                    ref FactoryProductionStat[] productionStats = ref GameMain.statistics.production.factoryStatPool;
                    int numOfSnapshots = reader.BinaryReader.ReadInt32();
                    for (int i = 0; i < numOfSnapshots; i++)
                    {
                        //Load new snapshot
                        snapshot = new StatisticalSnapShot(reader.BinaryReader);
                        for (int factoryId = 0; factoryId < snapshot.ProductionChangesPerFactory.Length; factoryId++)
                        {
                            if (productionStats[factoryId] == null)
                            {
                                productionStats[factoryId] = new FactoryProductionStat();
                                productionStats[factoryId].Init();
                            }
                            //Clear current statistical data
                            productionStats[factoryId].PrepareTick();

                            for (int changeId = 0; changeId < snapshot.ProductionChangesPerFactory[factoryId].Count; changeId++)
                            {
                                StatisticalSnapShot.ProductionChangeStruct productionChange = snapshot.ProductionChangesPerFactory[factoryId][changeId];
                                if (productionChange.IsProduction)
                                {
                                    productionStats[factoryId].productRegister[productionChange.ProductId] += productionChange.Amount;
                                }
                                else
                                {
                                    productionStats[factoryId].consumeRegister[productionChange.ProductId] += productionChange.Amount;
                                }
                            }
                            //Import power system statistics
                            productionStats[factoryId].powerGenRegister = snapshot.PowerGenerationRegister[factoryId];
                            productionStats[factoryId].powerConRegister = snapshot.PowerConsumptionRegister[factoryId];
                            productionStats[factoryId].powerChaRegister = snapshot.PowerChargingRegister[factoryId];
                            productionStats[factoryId].powerDisRegister = snapshot.PowerDischargingRegister[factoryId];

                            //Import fake energy stored values
                            Multiplayer.Session.Statistics.PowerEnergyStoredData = snapshot.EnergyStored;

                            //Import Research statistics
                            productionStats[factoryId].hashRegister = snapshot.HashRegister[factoryId];

                            //Processs changed registers. FactoryProductionStat.AfterTick() is empty currently so we ignore it.
                            productionStats[factoryId].GameTick(snapshot.CapturedGameTick);
                            itemChanged |= productionStats[factoryId].itemChanged;
                        }
                    }
                    //Trigger GameMain.statistics.production.onItemChange() event when itemChanged is true
                    if (itemChanged)
                    {
                        UIRoot.instance.uiGame.statWindow.OnItemChange();
                        NebulaModel.Logger.Log.Debug("StatisticsUpdateProcessor: itemChanged");
                    }
                }
            }
        }
    }
}
