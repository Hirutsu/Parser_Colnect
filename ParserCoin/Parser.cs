using Dapper;
using HtmlAgilityPack;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;

namespace ParserCoin
{
    public class Parser
    {
        private readonly Uri _uriWebSite;
        private readonly string _fileCountry;
        private readonly string _fileCatalogs;
        private readonly string _connectionString;
        private readonly string _imagesFolder;

        public int IdCoin { get; set; }
        public List<string> CountriesUrl { get; set; }
        public List<string> CatalogsUrl { get; set; }

        //текущие позиции
        private string _lastCatalogChecking;
        private int _indexCurCatalog;

        public Parser(string fileCountry, string fileCatalogs, Uri uriWebSite, string connectionString, string imagesForlder)
        {
            _connectionString = connectionString;
            IdCoin = GetLastId();
            _uriWebSite = uriWebSite;
            _fileCountry = fileCountry;
            _fileCatalogs = fileCatalogs;
            _imagesFolder = imagesForlder;
        }

        public async Task<bool> RunAsync()
        {
            try
            {
                //получаем ссылки на все страны; true - обновляем данные, false - берем из файла
                CountriesUrl = new();
                Console.WriteLine("Проверяем данные о странах");
                if (false)
                {
                    CountriesUrl = GetCountriesUrl();
                    SaveInFile(CountriesUrl, _fileCountry);
                    Console.WriteLine("Обновление списка стран прошло успешно");
                }
                else
                {
                    CountriesUrl = File.ReadAllLines(_fileCountry).ToList();
                    Console.WriteLine("Для обновления списка стран еще не прошло время, берем данные из файла");
                }

                //получаем ссылки на все каталоги в странах; true - обновляем данные, false - берем из файла
                CatalogsUrl = new();
                Console.WriteLine("Проверяем данные о каталогах в странах");
                if (false)
                {
                    CatalogsUrl = GetCatalogsUrl(CountriesUrl);
                    SaveInFile(CatalogsUrl, _fileCatalogs);
                    Console.WriteLine("Обновление каталогов в странах прошло успешно");
                }
                else
                {
                    CatalogsUrl = File.ReadAllLines(_fileCatalogs).ToList();
                    Console.WriteLine("Для обновления каталогов в странах еще не прошло время, берем данные из файла");
                }

                //получить ссылки на монеты из каталога
                Console.WriteLine("Получаем монеты из каталогов:");

                _indexCurCatalog = 0;
                foreach (var catalog in CatalogsUrl)
                {
                    _lastCatalogChecking = catalog;
                    Console.WriteLine($"Берем данные из каталога - {_lastCatalogChecking}");
                    List<DirtyCoin> coins = GetCoins(GetCoinsUrl(catalog));
                    await SaveCoinsSqlAsync(coins);
                    _indexCurCatalog++;
                    SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Message);
                Console.WriteLine($"Остановился на каталоге: {_lastCatalogChecking}");
                SaveChanges();

                return false;
            }
            return true;
        }

        private int GetLastId()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var querry = "SELECT max(id) FROM DirtyCoin";
                try
                {
                    return db.Query<int>(querry).FirstOrDefault() + 1;
                }
                catch (Exception e)
                {
                    return 1;
                }
            }
        }

        private void SaveChanges()
        {
            SaveInFile(CatalogsUrl.GetRange(_indexCurCatalog,CatalogsUrl.Count - _indexCurCatalog), _fileCatalogs);
        }

        private void SaveInFile(List<string> urls, string fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                foreach (var url in urls)
                {
                    sw.WriteLine(url.ToString());
                }
            }
        }

        private async Task SaveCoinsSqlAsync(List<DirtyCoin> coins)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                foreach (var coin in coins)
                {
                    var param = new DynamicParameters();
                    param.Add("Issuer", coin.Issuer);
                    param.Add("CatalogCode", coin.CatalogCode);
                    param.Add("Seria", coin.Seria);
                    param.Add("Topics", coin.Topics);
                    param.Add("ReleaseDate", coin.ReleaseDate);
                    param.Add("LastReleaseDate", coin.LastReleaseDate);
                    param.Add("Spreading", coin.Spreading);
                    param.Add("Thickness", coin.Thickness);
                    param.Add("Denomination", coin.Denomination);
                    param.Add("Mint", coin.Mint);
                    param.Add("Material", coin.Material);
                    param.Add("Gurt", coin.Gurt);
                    param.Add("Shape", coin.Shape);
                    param.Add("Weight", coin.Weight);
                    param.Add("Size", coin.Size);
                    param.Add("KnownCirculation", coin.KnownCirculation);
                    param.Add("MaterialDetails", coin.MaterialDetails);
                    param.Add("Mark", coin.Mark);
                    param.Add("SimilarMark", coin.SimilarMark);
                    param.Add("Description", coin.Description);
                    param.Add("Orientation", coin.Orientation);
                    param.Add("Kant", coin.Kant);
                    param.Add("ImgFrontUrl", coin.ImgFrontUrl);
                    param.Add("ImgBackUrl", coin.ImgBackUrl);

                    var querry = "INSERT INTO DirtyCoin " +
                        "([Issuer], [CatalogCode], [Seria], [Topics], [ReleaseDate], [LastReleaseDate], [Spreading], [Thickness], [Mint], [Material], [Gurt], [Shape], " +
                        "[Weight], [Size], [Denomination], [KnownCirculation], [MaterialDetails], [Mark], [SimilarMark], [Description], [Orientation], [Kant],[ImgFrontUrl],[ImgBackUrl]) " +
                        "VALUES " +
                        "(@Issuer, @CatalogCode, @Seria, @Topics, @ReleaseDate, @LastReleaseDate, @Spreading, @Thickness, @Mint, @Material, @Gurt, @Shape, " +
                        "@Weight, @Size, @Denomination, @KnownCirculation, @MaterialDetails, @Mark, @SimilarMark, @Description, @Orientation, @Kant, @ImgFrontUrl, @ImgBackUrl);";
                    await db.QueryAsync(querry, param);
                }
            }
            Console.WriteLine($"Данные о монетах из каталога {_lastCatalogChecking} сохранены в Базу Данных ParserColnect");
            Console.WriteLine();
        }

        private List<string> GetCountriesUrl()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_uriWebSite);
            request.Headers.Add(HttpRequestHeader.UserAgent, "PostmanRuntime/7.29.2");
            using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using Stream stream = response.GetResponseStream();

            HtmlDocument doc = new();
            doc.Load(stream);
            var result = doc.DocumentNode.SelectNodes("//a[contains(@class, 'country_flag')]");

            List<string> urlsCountries = new();

            foreach (var item in result)
            {
                urlsCountries.Add(_uriWebSite.Scheme + "://" + _uriWebSite.Host + item.GetAttributeValue("href", ""));
            }

            return urlsCountries;
        }

        private List<string> GetCatalogsUrl(List<string> listUriCountries)
        {
            List<string> urlsCatalogCountries = new();

            foreach (var uriItem in listUriCountries)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriItem);
                request.Headers.Add(HttpRequestHeader.UserAgent, "PostmanRuntime/7.29.2");
                using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using Stream stream = response.GetResponseStream();

                HtmlDocument doc = new();
                doc.Load(stream);

                var result = doc.DocumentNode.SelectNodes("//*[contains(@id, 'pl_500')]/a");

                if (result != null)
                {
                    foreach (var item in result)
                    {
                        urlsCatalogCountries.Add(_uriWebSite.Scheme + "://" + _uriWebSite.Host + item.GetAttributeValue("href", ""));
                    }
                }
                Thread.Sleep(2500);
            }
            return urlsCatalogCountries;
        }


        private List<string> GetCoinsUrl(string listUriCatalogsCountries)
        {
            List<string> coinsDiv = new();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(listUriCatalogsCountries);
            request.Headers.Add(HttpRequestHeader.UserAgent, "PostmanRuntime/7.29.2");
            using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using Stream stream = response.GetResponseStream();

            HtmlDocument doc = new();
            doc.Load(stream);

            var result = doc.DocumentNode.SelectNodes("//h2[contains(@class, 'item_header')]/a");

            if (result != null)
            {
                foreach (var item in result)
                {
                    coinsDiv.Add(_uriWebSite.Scheme + "://" + _uriWebSite.Host + item.GetAttributeValue("href", ""));
                }
            }

            return coinsDiv;
        }

        private List<DirtyCoin> GetCoins(List<string> urlsCoin)
        {
            List<DirtyCoin> coins = new();

            foreach (var uriItem in urlsCoin)
            {
                Console.WriteLine($"Берем данные о монете: {uriItem}");
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriItem);
                request.Headers.Add(HttpRequestHeader.UserAgent, "PostmanRuntime/7.29.2");
                using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using Stream stream = response.GetResponseStream();

                HtmlDocument doc = new();
                doc.Load(stream);

                DirtyCoin dirtyCoin = new();

                for (int index = 0; index < 25; index++)
                {
                    switch (doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_full_details')]/div[1]/dl/dt[{index}]")?.FirstOrDefault()?.InnerHtml)
                    {
                        case "Страна:":
                            dirtyCoin.Issuer = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_full_details')]/div[1]/dl/dd[{index}]/a/text()")?.FirstOrDefault()?.InnerHtml;
                            break;
                        case "Код по каталогу:":
                            dirtyCoin.CatalogCode = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_full_details')]/div[1]/dl/dd[{index}]/a")?.FirstOrDefault()?.InnerHtml;
                            break;
                        case "Серия:":
                            dirtyCoin.Seria = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_full_details')]/div[1]/dl/dd[{index}]/a")?.FirstOrDefault()?.InnerHtml;
                            break;
                        case "Темы:":
                            dirtyCoin.Topics = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_full_details')]/div[1]/dl/dd[3]/a[1]")?.FirstOrDefault()?.InnerHtml;
                            break;
                        case "Дата выпуска:":
                            dirtyCoin.ReleaseDate = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_full_details')]/div[1]/dl/dd[{index}]/a")?.FirstOrDefault()?.InnerHtml;
                            break;
                        case "Последний год выпуска:":
                            dirtyCoin.LastReleaseDate = doc.DocumentNode.SelectNodes($"//*[contains (@id, 'item_full_details')]/div[1]/dl/dd[{index}]/a")?.FirstOrDefault()?.InnerHtml;
                            break;
                        case "Распространение:":
                            dirtyCoin.Spreading = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_full_details')]/div[1]/dl/dd[{index}]/a")?.FirstOrDefault()?.InnerHtml;
                            break;
                        case "Монетные дворы:":
                            dirtyCoin.Mint = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_full_details')]/div[1]/dl/dd[{index}]/a")?.FirstOrDefault()?.InnerHtml;
                            break;
                        case "Состав:":
                            dirtyCoin.Material = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_full_details')]/div[1]/dl/dd[{index}]/a")?.FirstOrDefault()?.InnerHtml;
                            break;
                        case "Гурт:":
                            dirtyCoin.Gurt = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_full_details')]/div[1]/dl/dd[{index}]/a")?.FirstOrDefault()?.InnerHtml;
                            break;
                        case "Ориентация:":
                            dirtyCoin.Orientation = doc.DocumentNode.SelectNodes($"//*[contains(@id , 'item_full_details')]/div[1]/dl/dd[{index}]/a")?.FirstOrDefault()?.InnerHtml;
                            break;
                        case "Форма:":
                            dirtyCoin.Shape = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_full_details')]/div[1]/dl/dd[{index}]/a")?.FirstOrDefault()?.InnerHtml;
                            break;
                        case "Кант/Буртик:":
                            dirtyCoin.Kant = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_full_details')]/div[1]/dl/dd[{index}]/a")?.FirstOrDefault()?.InnerHtml;
                            break;
                        case "Вес:":
                            dirtyCoin.Weight = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_full_details')]/div[1]/dl/dd[{index}]")?.FirstOrDefault()?.InnerHtml;
                            break;
                        case "Размер":
                            dirtyCoin.Size = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_full_details')]/div[1]/dl/dd[{index}]")?.FirstOrDefault()?.InnerHtml;
                            break;
                        case "Толщина":
                            dirtyCoin.Thickness = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_full_details')]/div[1]/dl/dd[{index}]")?.FirstOrDefault()?.InnerHtml;
                            break;
                        case "Номинальная стоимость:":
                            dirtyCoin.Denomination = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_full_details')]/div[1]/dl/dd[{index}]/text()")?.FirstOrDefault()?.InnerHtml;
                            string nominalName = doc.DocumentNode.SelectNodes($"//*[contains(@id,'item_full_details')]/div[1]/dl/dd[16]/span")?.FirstOrDefault()?.InnerHtml;
                            dirtyCoin.Denomination += " " + nominalName;
                            break;
                        case "Известный тираж:":
                            dirtyCoin.KnownCirculation = doc.DocumentNode.SelectNodes($"//*[contains(@id,'item_full_details')]/div[1]/dl/dd[{index}]")?.FirstOrDefault()?.InnerHtml;
                            break;
                        case "Подробности Состава:":
                            dirtyCoin.MaterialDetails = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_full_details')]/div[1]/dl/dd[{index}]")?.FirstOrDefault()?.InnerHtml;
                            break;
                        case "Оценка:":
                            dirtyCoin.Mark = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_full_details')]/div[1]/dl/dd[{index}]")?.FirstOrDefault()?.InnerHtml;
                            break;
                        case "Cхожие оценки:":
                            dirtyCoin.SimilarMark = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_full_details')]/div[1]/dl/dd[{index}]")?.FirstOrDefault()?.InnerHtml;
                            break;
                        case "Описание:":
                            dirtyCoin.Description = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_full_details')]/div[1]/dl/dd[{index}]")?.FirstOrDefault()?.InnerHtml;
                            break;
                        default:
                            break;
                    }

                    var imgFront = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_pics_row')]/div[1]/img")?.FirstOrDefault();
                    if (imgFront != null)
                    {
                        var imgFrontUrl = @$"{_imagesFolder}\{IdCoin}_front.webp";
                        using (WebClient client = new WebClient())
                        {
                            client.Headers.Add(HttpRequestHeader.UserAgent, "PostmanRuntime/7.29.2");
                            using (Stream streamImg = client.OpenRead("http://" + imgFront?.GetAttributeValue("src", "")[2..]))
                            {
                                Bitmap bitmap = new Bitmap(streamImg);
                                if (bitmap != null)
                                {
                                    Image image = bitmap;
                                    image.Save(imgFrontUrl, ImageFormat.Webp);
                                }
                            }
                        }
                        dirtyCoin.ImgFrontUrl = imgFrontUrl;
                    }

                    var imgBack = doc.DocumentNode.SelectNodes($"//*[contains(@id, 'item_pics_row')]/div[2]/img")?.FirstOrDefault();

                    if (imgBack != null)
                    {
                        var imgBackUrl = @$"{_imagesFolder}\{IdCoin}_back.webp";
                        using (WebClient client = new())
                        {
                            client.Headers.Add(HttpRequestHeader.UserAgent, "PostmanRuntime/7.29.2");
                            using (Stream streamImg = client.OpenRead("http://" + imgBack?.GetAttributeValue("src", "")[2..]))
                            {
                                Bitmap bitmap = new(streamImg);
                                if (bitmap != null)
                                {
                                    Image image = bitmap;
                                    image.Save(imgBackUrl, ImageFormat.Webp);
                                }
                            }
                        }
                        dirtyCoin.ImgBackUrl = imgBackUrl;
                    }
                }
                coins.Add(dirtyCoin);
                IdCoin++;
            }
            return coins;
        }
    }
}
