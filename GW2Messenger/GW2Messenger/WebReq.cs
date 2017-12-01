using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace GW2Messenger
{
    internal class WebReq
    {
        public static Gw2Data.Outfit[] UpdateOutfitList()
        {
            var reg = new Regex(@"u\D+(\d+).+i\D+(\d+).+n.+""(\D+)"".+p\D+(\d+)");
            var outfits = new List<Gw2Data.Outfit>();

            var webRequest = WebRequest.Create(@"http://gw2timer.com/data/outfits.js");
            try
            {
                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                {
                    if (content is null)
                        return null;

                    using (var reader = new StreamReader(content))
                    {
                        while (!reader.EndOfStream)
                        {
                            var s = reader.ReadLine();
                            if (s is null) return null;

                            var mat = reg.Match(s);
                            if (!mat.Success)
                                continue;
                            if (mat.Groups.Count != 5)
                                continue;

                            outfits.Add(new Gw2Data.Outfit
                            {
                                UnlockableId = int.Parse(mat.Groups[1].Value),
                                Id = int.Parse(mat.Groups[2].Value),
                                Name = mat.Groups[3].Value,
                                GemCost = int.Parse(mat.Groups[4].Value)
                            });
                        }
                        return outfits.ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static Gw2Data.Sale[] UpdateGemSaleList()
        {
            var reg = new Regex(@"i\D+(\d+)\D+n\D+""(\D+)""\D+p\D+(\d+)");
            var regFinish = new Regex(@"Finish: new Date\(""(.+)""");
            var regDiscount = new Regex(@"discount: \[(.+)\]");
            var sales = new List<Gw2Data.Sale>();
            
            var webRequest = WebRequest.Create(@"http://gw2timer.com/data/sale.js");
            try
            {
                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                {
                    if (content is null)
                        return null;

                    using (var reader = new StreamReader(content))
                    {
                        while (!reader.EndOfStream)
                        {
                            var s = reader.ReadLine();
                            if (s is null) return null;

                            var mat = reg.Match(s);
                            if (!mat.Success)
                                continue;
                            if (mat.Groups.Count != 4)
                                continue;

                            var sale = new Gw2Data.Sale
                            {
                                Id = int.Parse(mat.Groups[1].Value),
                                Name = mat.Groups[2].Value,
                                GemCost = int.Parse(mat.Groups[3].Value)
                            };
                            var finishmat = regFinish.Match(s);
                            if (finishmat.Success && finishmat.Groups.Count == 2)
                                sale.EndDate = finishmat.Groups[1].Value;
                            var discountmat = regDiscount.Match(s);
                            if (discountmat.Success && discountmat.Groups.Count == 2)
                                sale.GemDiscountArray = discountmat.Groups[1].Value;

                            sales.Add(sale);
                        }
                        return sales.ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Checks sever status of desolation
        /// </summary>
        /// <returns><para>null: API down or no results</para>
        /// <para>false: Server full
        /// <para>true: Server not full</para></para></returns>
        public bool? UpdateServerPopulation()
        {
            var reg = new Regex(@"(\w+)");
            var webRequest = WebRequest.Create(@"https://api.guildwars2.com/v2/worlds?ids=2002");
            try
            {
                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                {
                    if (content is null)
                    {
                        return null;
                    }
                    
                    using (var reader = new StreamReader(content))
                    {
                        while (!reader.EndOfStream)
                        {
                            var s = reader.ReadLine();
                            if (s is null) return null;
                            var res = reg.Matches(s);

                            if (res.Count != 2) continue;
                            if (res[0].Value != "population") continue;

                            return res[1].Value == "Full";
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }
    }
}
