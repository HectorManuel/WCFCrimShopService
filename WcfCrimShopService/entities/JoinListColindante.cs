using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;

namespace WcfCrimShopService.entities
{
    public class JoinListColindante
    {
        public List<Objects.OrderItemList> JoinMultipleList(List<Objects.OrderItemList> list)
        {
            List<Objects.OrderItemList> finalList = new List<Objects.OrderItemList>();

            for (int i = 0; i < list.Count(); i++)
            {
                string reg = string.Empty;
                try
                {
                    reg = Regex.Replace(list[i].itemName, @"(_\d+)", "", RegexOptions.None, TimeSpan.FromSeconds(2));
                }
                catch
                {
                    reg = list[i].itemName;
                }
                list[i].itemName = reg;

            }

            finalList = QueryList(list);

            return finalList;
        }

        private List<Objects.OrderItemList> QueryList(List<Objects.OrderItemList> list)
        {
            List<Objects.OrderItemList> finalList = new List<Objects.OrderItemList>();
            List<Objects.OrderItemList> listToJoin = new List<Objects.OrderItemList>();
            List<string> namesVerified = new List<string>();

            foreach (var item in list)
            {
                int index = namesVerified.IndexOf(item.itemName);
                
                if(index == -1){
                    var items = from colindante in list
                                where colindante.itemName == item.itemName && colindante.itemQty == item.itemQty
                                select colindante;
                    if (items.Count() > 1)
                    {
                        foreach (var listItem in items)
                        {
                            listToJoin.Add(new Objects.OrderItemList
                            {
                                ControlNumber = listItem.ControlNumber,
                                itemName = listItem.itemName,
                                itemQty = listItem.itemQty,
                                item = listItem.item,
                                cost = listItem.cost,
                                created = listItem.created
                            });
                        }

                        Objects.OrderItemList newObject = StartJoining(listToJoin);
                        finalList.Add(newObject);
                        namesVerified.Add(item.itemName);

                    }
                    else if (items.Count() == 1)
                    {
                        foreach (var listItem in items)
                        {
                            finalList.Add(new Objects.OrderItemList
                            {
                                ControlNumber = listItem.ControlNumber,
                                itemName = listItem.itemName,
                                itemQty = listItem.itemQty,
                                item = listItem.item,
                                cost = listItem.cost,
                                created = listItem.created
                            });

                            namesVerified.Add(item.itemName);
                        }
                    }
                }  
            }


            return finalList;
        }

        /// <summary>
        /// Join a list composed of multiple lists into one create one large list of adjacent parcels.
        /// </summary>
        /// <param name="list"></param>
        /// <returns>single object with list</returns>
        private Objects.OrderItemList StartJoining(List<Objects.OrderItemList> list)
        {
            Objects.OrderItemList newJoinList = list[0];

            Objects.ListaCol listColindante = JsonConvert.DeserializeObject<Objects.ListaCol>(list[0].item);


            //add the rest of the items to the list 
            for (int i = 1; i < list.Count(); i++)
            {
                Objects.ListaCol otherList = JsonConvert.DeserializeObject<Objects.ListaCol>(list[i].item);

                foreach (var it in otherList.ListaColindante)
                {
                    listColindante.ListaColindante.Add(it);
                }
            }

            string itemSerialize = JsonConvert.SerializeObject(listColindante);

            newJoinList.item = itemSerialize;

            return newJoinList;
        }
    }
}