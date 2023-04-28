using MahApps.Metro.Controls;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using wp13_price.Logics;
using wp13_price.Models;

namespace wp13_price
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BtnReqRealtime_Click(object sender, RoutedEventArgs e)
        {
            string openApiUri = "https://apis.data.go.kr/6260000/EgMarketCost/getDailyCost?serviceKey=DcPRnwI4pwBpTF1s7u4ysuLiKPdok7H0Rygeg4oBEW%2FpYcSPz7eKA1c9D%2FubqFt3KQauBSOqPA816gHzRtpR8w%3D%3D&resultType=json";
            string result = string.Empty;

            // WebRequest, WebResponse 객체
            WebRequest req = null;
            WebResponse res = null;
            StreamReader reader = null;

            try
            {
                req = WebRequest.Create(openApiUri);
                res = await req.GetResponseAsync();
                reader = new StreamReader(res.GetResponseStream());
                result = reader.ReadToEnd();

                Debug.WriteLine(result);
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"OpenAPI 조회오류 {ex.Message}");
            }
            var jsonResult = JObject.Parse(result);
            var resultCode = Convert.ToString(jsonResult["getDailyCost"]["header"]["resultCode"]);

            try
            {
                if (resultCode == "00")      // 정상이면 데이터받아서 처리
                {
                    var data = jsonResult["getDailyCost"]["body"]["items"]["item"];
                    var json_array = data as JArray;

                    var MarketPrice = new List<MarketPrice>();
                    foreach (var sensor in json_array)
                    {
                        MarketPrice temp = new MarketPrice
                        {
                            MidName = Convert.ToString(sensor["midName"]),
                            GoodName = Convert.ToString(sensor["goodName"]),
                            Danq = Convert.ToDouble(sensor["danq"]),
                            Dan = Convert.ToString(sensor["dan"]),a
                            Poj = Convert.ToString(sensor["poj"]),
                            SizeName = Convert.ToString(sensor["sizeName"]),
                            Lv = Convert.ToString(sensor["lv"]),
                            MinCost = Convert.ToInt32(sensor["minCost"]),
                            MaxCost = Convert.ToInt32(sensor["maxCost"]),
                            AveCost = Convert.ToInt32(sensor["aveCost"]),
                            Saledate = Convert.ToDateTime(sensor["saledate"]),
                            CmpName = Convert.ToString(sensor["cmpName"]),
                            LargeName = Convert.ToString(sensor["largeName"]),

                        };
                        MarketPrice.Add(temp);
                    }

                    this.DataContext = MarketPrice;
                    StsResult.Content = $"OpenAPI {MarketPrice.Count} 건 조회완료";
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"JSON 처리오류 {ex.Message}");
            }


        }

        private async void BtnSaveData_Click(object sender, RoutedEventArgs e)
        {
            if (GrdResult.Items.Count == 0)
            {
                await Commons.ShowMessageAsync("오류", "조회쫌하고 저장하세요.");
                return;
            }
            try
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.myConnString))
                {
                    if (conn.State == System.Data.ConnectionState.Closed) conn.Open();
                    var query = @"INSERT INTO dustsensor
                                            (
                                            Dev_id,
                                            Name,
                                            Loc,
                                            Coordx,
                                            Coordy,
                                            Ison,
                                            Pm10_after,
                                            Pm25_after,
                                            State,
                                            Timestamp,
                                            Company_id,
                                            Company_name)
                                            VALUES
                                            (
                                            @Dev_id,
                                            @Name,
                                            @Loc,
                                            @Coordx,
                                            @Coordy,
                                            @Ison,
                                            @Pm10_after,
                                            @Pm25_after,
                                            @State,
                                            @Timestamp,
                                            @Company_id,
                                            @Company_name)";

                    var insRes = 0;
                    foreach (var temp in GrdResult.Items)
                    {
                        if (temp is MarketPrice)
                        {
                            var item = temp as MarketPrice;


                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@MidName", item.MidName);
                            cmd.Parameters.AddWithValue("@GoodName", item.GoodName);
                            cmd.Parameters.AddWithValue("@Danq", item.Danq);
                            cmd.Parameters.AddWithValue("@Dan", item.Dan);
                            cmd.Parameters.AddWithValue("@Poj", item.Poj);
                            cmd.Parameters.AddWithValue("@SizeName", item.SizeName);
                            cmd.Parameters.AddWithValue("@Lv", item.Lv);
                            cmd.Parameters.AddWithValue("@MinCost", item.MinCost);
                            cmd.Parameters.AddWithValue("@MaxCost", item.MaxCost);
                            cmd.Parameters.AddWithValue("@AveCost", item.AveCost);
                            cmd.Parameters.AddWithValue("@Saledate", item.Saledate);
                            cmd.Parameters.AddWithValue("@CmpName", item.CmpName);
                            cmd.Parameters.AddWithValue("@LargeName", item.LargeName);

                            insRes += cmd.ExecuteNonQuery();

                        }
                    }
                    await Commons.ShowMessageAsync("저장", "DB저장 성공");
                    StsResult.Content = $"DB저장 {insRes}건 성공";
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB저장 오류!{ex.Message}");

            }
        }
    }
}
