using CodeHollow.FeedReader;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace FeedReader
{
    internal class FeedsDatabaseManager
    {

        public static bool CreateDB()
        {
            try
            {
                using (var connection = new SqliteConnection("Data Source=feedsDB.db"))
                {
                    connection.Open();
                    SqliteCommand command = new SqliteCommand();
                    command.Connection = connection;
                    command.CommandText = @"CREATE TABLE IF NOT EXISTS feeds(id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, source TEXT NOT NULL, source_name TEXT NOT NULL, link TEXT NOT NULL, title TEXT NOT NULL, content TEXT, is_read INT, is_deleted INT, is_trully_deleted INT, publishing_date TEXT);
                                            CREATE TABLE IF NOT EXISTS feeds_list(id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, source TEXT NOT NULL, source_name TEXT NOT NULL);";
                    if (command.ExecuteNonQuery() == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static List<FeedSource> GetFeedsSources()
        {
            List<FeedSource> sources = new List<FeedSource>();
            try
            {
                using (var connection = new SqliteConnection("Data Source=feedsDB.db"))
                {
                    connection.Open();
                    SqliteCommand command = new SqliteCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT source, source_name FROM feeds_list";
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                FeedSource fs = new FeedSource();
                                fs.Source = reader.GetValue(0).ToString();
                                fs.SourceName = reader.GetValue(1).ToString().Trim();
                                sources.Add(fs);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return sources;
        }

        public static async Task UpdateFeeds()
        {
            var sources = GetFeedsSources();
            foreach(FeedSource source in sources)
            {
                try
                {
                    var feed = await CodeHollow.FeedReader.FeedReader.ReadAsync(source.Source);
                    foreach (var item in feed.Items)
                    {
                        var feedItem = new Feed();
                        feedItem.Source = feed.Link;
                        feedItem.SourceName = source.SourceName.Trim();
                        feedItem.Link = item.Link;
                        feedItem.Title = item.Title;
                        feedItem.Content = item.Description;
                        feedItem.IsDeleted = 0;
                        feedItem.IsTrullyDeleted = 0;
                        feedItem.Date = item.PublishingDate?.AddHours(5);
                        FeedsDatabaseManager.InsertFeed(feedItem);
                    }
                }
                catch { }
            }
        }
        public static void InsertFeedSource(string source)
        {
            try
            {
                bool isThere=false;
                using (var connection = new SqliteConnection("Data Source=feedsDB.db"))
                {
                    connection.Open();
                    SqliteCommand command = new SqliteCommand();
                    command.Connection = connection;

                    command.CommandText = "SELECT source FROM feeds_list WHERE source=\"" + source + "\"";
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            isThere = true;
                        }
                    }
                }
                if (!isThere)
                {
                    string sourceName = source;
                    
                    try
                    {
                        try
                        {
                            var f = CodeHollow.FeedReader.FeedReader.Read(source);
                            sourceName = f.Title;
                        }
                        catch 
                        {
                            try
                            {
                                var urls = CodeHollow.FeedReader.FeedReader.GetFeedUrlsFromUrl(source);
                                if (urls.Count() < 1)
                                {
                                    sourceName = source;
                                }
                                else if (urls.Count() == 1)
                                {
                                    sourceName = urls.First().Url;
                                }
                                else if (urls.Count() == 2)
                                {
                                    sourceName = urls.First().Url;
                                }
                            }
                            catch { }
                        }
                        /*if (String.IsNullOrEmpty(sourceName))
                        {
                            var urls = CodeHollow.FeedReader.FeedReader.GetFeedUrlsFromUrl(source);
                            if (urls.Count() < 1)
                            {
                                sourceName = source;
                            }
                            else if(urls.Count() == 1)
                            {
                                sourceName = urls.First().Url;
                            }   
                            else if (urls.Count() == 2)
                            {
                                sourceName = urls.First().Url;
                            }
                        }*/
                        /*if (f.Title == "")
                        {
                            return;
                        }*/
                    }
                    catch 
                    {
                        /*return;*/
                    }
                    using (var connection = new SqliteConnection("Data Source=feedsDB.db"))
                    {
                        connection.Open();
                        SqliteCommand command = new SqliteCommand();
                        command.Connection = connection;
                        command.CommandText = "INSERT INTO feeds_list (source, source_name) VALUES(@source, @source_name)";
                        command.Parameters.AddWithValue("source", source);
                        command.Parameters.AddWithValue("source_name", sourceName);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
            }
        }

        public static void DelFeedSource(string source, string sourceName)
        {
            try
            {
                using (var connection = new SqliteConnection("Data Source=feedsDB.db"))
                {
                    connection.Open();
                    SqliteCommand command = new SqliteCommand();
                    command.Connection = connection;
                    command.CommandText = "DELETE FROM feeds_list WHERE source=\"" + source + "\"";
                    command.ExecuteNonQuery();
                }

                using (var connection = new SqliteConnection("Data Source=feedsDB.db"))
                {
                    connection.Open();
                    SqliteCommand command = new SqliteCommand();
                    command.Connection = connection;
                    command.CommandText = "UPDATE feeds SET is_deleted = 1 WHERE source_name=\"" + sourceName + "\"";
                    command.ExecuteNonQuery();
                }
            }
            catch
            {
            }
        }

        /*public static List<string> GetSources()
        {
            List<string> sources = new List<string>();
            try
            {
                using (var connection = new SqliteConnection("Data Source=feedsDB.db"))
                {
                    connection.Open();
                    SqliteCommand command = new SqliteCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT source FROM feeds_list";
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                string source = reader.GetValue(0).ToString();
                                sources.Add(source);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return sources;
        }*/
        private static bool IsThere(string link)
        {
            try
            {
                using (var connection = new SqliteConnection("Data Source=feedsDB.db"))
                {
                    connection.Open();
                    SqliteCommand command = new SqliteCommand();
                    command.Connection = connection;
                    command.CommandText = "SELECT * FROM feeds WHERE link=\"" + link + "\"";
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        public static void InsertFeed(Feed feed)
        {
            try
            {
                if (!IsThere(feed.Link))
                {
                    using (var connection = new SqliteConnection("Data Source=feedsDB.db"))
                    {
                        connection.Open();
                        SqliteCommand command = new SqliteCommand();
                        command.Connection = connection;
                        command.CommandText = "INSERT INTO feeds (source, source_name, link, title, content, is_read, is_deleted, is_trully_deleted, publishing_date) VALUES (@source, @source_name, @link, @title, @content, @is_read, @is_deleted, @is_trully_deleted, @publishing_date)";
                        command.Parameters.AddWithValue("source", feed.Source);
                        command.Parameters.AddWithValue("source_name", feed.SourceName);
                        command.Parameters.AddWithValue("link", feed.Link);
                        command.Parameters.AddWithValue("title", feed.Title);
                        command.Parameters.AddWithValue("content", feed.Content);
                        command.Parameters.AddWithValue("is_read", feed.IsRead);
                        command.Parameters.AddWithValue("is_deleted", feed.IsDeleted);
                        command.Parameters.AddWithValue("is_trully_deleted", feed.IsTrullyDeleted);
                        command.Parameters.AddWithValue("publishing_date", feed.Date?.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
            }
        }

        public static void DelFeed(string feedLink)
        {
            try
            {
                using (var connection = new SqliteConnection("Data Source=feedsDB.db"))
                {
                    connection.Open();
                    SqliteCommand command = new SqliteCommand();
                    command.Connection = connection;
                    command.CommandText = "UPDATE feeds SET is_deleted=1 WHERE link='" + feedLink + "'";
                    /*if (!trash)
                    {
                        command.CommandText = "UPDATE feeds SET is_deleted=1 WHERE link='" + feedLink + "'";
                    }
                    else
                    {
                        command.CommandText = "DELETE FROM feeds WHERE is_deleted=1";
                    }*/
                    command.ExecuteNonQuery();
                }
            }
            catch
            {
            }
        }

        public static void TrullyDelFeed(string feedLink)
        {
            try
            {
                using (var connection = new SqliteConnection("Data Source=feedsDB.db"))
                {
                    connection.Open();
                    SqliteCommand command = new SqliteCommand();
                    command.Connection = connection;
                    command.CommandText = "UPDATE feeds SET is_trully_deleted=1 WHERE link='" + feedLink + "'";
                    /*if (!trash)
                    {
                        command.CommandText = "UPDATE feeds SET is_deleted=1 WHERE link='" + feedLink + "'";
                    }
                    else
                    {
                        command.CommandText = "DELETE FROM feeds WHERE is_deleted=1";
                    }*/
                    command.ExecuteNonQuery();
                }
            }
            catch
            {
            }
        }

        public static void CleanGarbage()
        {
            try
            {
                using (var connection = new SqliteConnection("Data Source=feedsDB.db"))
                {
                    connection.Open();
                    SqliteCommand command = new SqliteCommand();
                    command.Connection = connection;
                    string date = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
                    command.CommandText = "DELETE FROM feeds WHERE is_trully_deleted=1 AND date(publishing_date) < '"+date+"';";
                    command.ExecuteNonQuery();
                }
            }
            catch
            {
            }
        }

        public static void MarkAsRead(string feedLink)
        {
            try
            {
                using (var connection = new SqliteConnection("Data Source=feedsDB.db"))
                {
                    connection.Open();
                    SqliteCommand command = new SqliteCommand();
                    command.Connection = connection;
                    command.CommandText = "UPDATE feeds SET is_read=1 WHERE link='"+feedLink+"'";
                    command.ExecuteNonQuery();
                }
            }
            catch
            {
            }
        }
        public static int GetUnreadNumber(string sourceName="")
        {
            int number = 0;

            try
            {
                using (var connection = new SqliteConnection("Data Source=feedsDB.db"))
                {
                    connection.Open();
                    SqliteCommand command = new SqliteCommand();
                    command.Connection = connection;
                    if(sourceName != "")
                    {
                        command.CommandText = "SELECT count(*) FROM feeds WHERE source_name='" + sourceName + "' AND is_read=0 AND is_deleted=0";
                    }
                    else
                    {
                        command.CommandText = "SELECT count(*) FROM feeds WHERE is_read=0 AND is_deleted=0";
                    }
                    try
                    {
                        number = (int)(long)command.ExecuteScalar();
                    }
                    catch { }
                    /*using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                FeedSource fs = new FeedSource();
                                fs.Source = reader.GetValue(0).ToString();
                                fs.SourceName = reader.GetValue(1).ToString();
                                sources.Add(fs);
                            }
                        }
                    }*/
                }
            }
            catch
            {
            }

            return number;
        }
        public static List<Feed> GetFeeds(string source="")
        {
            List<Feed> feeds = new List<Feed>();
            try
            {
                using (var connection = new SqliteConnection("Data Source=feedsDB.db"))
                {
                    connection.Open();
                    SqliteCommand command = new SqliteCommand();
                    command.Connection = connection;
                    if(source != "")
                    {
                        if (source == "trash")
                        {
                            command.CommandText = "SELECT * FROM feeds WHERE is_deleted=1 AND is_trully_deleted=0 ORDER BY publishing_date DESC;";
                        }
                        else
                        {
                            command.CommandText = "SELECT * FROM feeds WHERE source_name='" + source + "' AND is_deleted=0 AND is_trully_deleted=0 ORDER BY publishing_date DESC;";
                        }
                    }
                    else
                    {
                        command.CommandText = "SELECT * FROM feeds WHERE is_deleted=0 AND is_trully_deleted=0 ORDER BY publishing_date DESC;";
                    }
                    using(SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Feed feed = new Feed();
                                feed.Source = reader.GetValue(1).ToString();
                                feed.SourceName = reader.GetValue(2).ToString();
                                feed.Link = reader.GetValue(3).ToString();
                                feed.Title= reader.GetValue(4).ToString();
                                feed.Content= reader.GetValue(5).ToString();
                                feed.IsRead= int.Parse(reader.GetValue(6).ToString());
                                feed.IsDeleted= int.Parse(reader.GetValue(7).ToString());
                                feed.IsTrullyDeleted = int.Parse(reader.GetValue(8).ToString());
                                feed.Date = DateTime.Parse(reader.GetValue(9).ToString());
                                feeds.Add(feed);
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return feeds;
        }
    }
}
