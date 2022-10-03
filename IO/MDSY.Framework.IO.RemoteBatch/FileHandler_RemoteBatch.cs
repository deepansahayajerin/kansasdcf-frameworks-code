using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using MDSY.Framework.Buffer.Interfaces;
using MDSY.Framework.IO.Common;


namespace MDSY.Framework.IO.RemoteBatch
{
    public class FileHandler_RemoteBatch : IFileHandler
    {

        #region private fields

        private IDictionary<string, Common.IFileLink> files { get; set; }
        private const string LOG_PREFIX = "MDSY_IO";
        const string envVariableName = "RuntimeJobStepId";
        const string envVariableConnectionString = "RuntimeConnectionString";
        bool isCatalogInDB = false;
        string jobIdValue;
        string batchRuntimeConnectionString;

        #endregion

        #region constructors

        public FileHandler_RemoteBatch()
        {
            Initialize();
        }

        #endregion

        #region public properties

        public IDictionary<string, Common.IFileLink> Files
        {
            get
            {
                // Create on demand...
                if (files == null)
                    files = new Dictionary<string, Common.IFileLink>();
                return files;
            }
            set
            {
                files = value;
            }

        }

        public bool IsDebugging { get; set; }

        public Common.IFileLink this[string index]
        {
            get
            {
                int lastIndexPos = index.LastIndexOf('_');

                string ddName = (lastIndexPos > 0) ?
                     index.Substring(lastIndexPos + 1) :
                     index;

                LoadFile(ddName);

                return Files[ddName];
            }
        }

        #endregion

        #region public methods

        public void Add(Common.IFileLink fileLink)
        {
            if (!Files.ContainsKey(fileLink.DDName))
            {
                Files.Add(fileLink.DDName.Trim(), fileLink);
            }
        }

        public IFileLink GetFile(string logicalFileName)
        {
            //Physical filename
            int lastDash = logicalFileName.LastIndexOf('_');
            string ddName;
            if (lastDash > 0)
                ddName = logicalFileName.Substring(lastDash + 1);
            else
                ddName = logicalFileName;

            FileLink_RemoteBatch file;
            if (Files.ContainsKey(ddName.Trim()))
            {
                file = (FileLink_RemoteBatch)Files[ddName.Trim()];
                ICatalogFileInfo tempCatInfo = GetCatalogFileInfo(file.CatalogFileName);
                if (tempCatInfo != null)
                {
                    file.FileOrganization = tempCatInfo.CatalogFileOrganization;
                    file.FileType = tempCatInfo.CatalogFileType;
                    file.FilePath = tempCatInfo.CatalogFilePath;
                    file.RecordLength = tempCatInfo.CatalogRecordLength;
                    file.VsamSegmentName = tempCatInfo.VsamSegmentName;
                    file.VsamKeys = tempCatInfo.VsamKeys;
                                                                                            
                   
                }
                return file;
            }
            else
            {
                FileLink_RemoteBatch FJ = new FileLink_RemoteBatch(this, ddName.Trim());
                ICatalogFileInfo tempCatInfo = GetCatalogFileInfo(ddName);
                if (tempCatInfo != null)
                {
                    FJ.FileOrganization = tempCatInfo.CatalogFileOrganization;
                    FJ.FileType = tempCatInfo.CatalogFileType;
                    FJ.FilePath = tempCatInfo.CatalogFilePath;
                    FJ.RecordLength = tempCatInfo.CatalogRecordLength;
                    FJ.VsamSegmentName = tempCatInfo.VsamSegmentName;
                    FJ.VsamKeys = tempCatInfo.VsamKeys;
                    FJ.FileStatus = FileStatus.Successful_completion;
                    FJ.ReadPointer = 0;
                }
                else
                {
                    FJ.FileStatus = FileStatus.File_doesnt_exist;
                    //Console.Write("File " + ddName + " information not found in Enviromental variables!");
                    //throw new ApplicationException("File " + ddName + " information not found in Enviromental variables!");
                }
                return FJ;
            }
        }

        /// <summary>
        /// USing implementation for Dynamic files
        /// </summary>
        /// <param name="dynamicFileName"></param>
        /// <returns></returns>
        public IFileLink GetFileUsing(IBufferValue dynamicFileName)
        {

            if (dynamicFileName.BytesAsString.Trim() == string.Empty)
            {
                dynamicFileName.Assign("Dynamic");
            }

            FileLink_RemoteBatch FJ = new FileLink_RemoteBatch(this, dynamicFileName.BytesAsString);
            FJ.DynamicFileName = dynamicFileName;
            ICatalogFileInfo tempCatInfo = GetCatalogFileInfo(dynamicFileName.BytesAsString);
            if (tempCatInfo != null)
            {
                FJ.FileOrganization = tempCatInfo.CatalogFileOrganization;
                FJ.FileType = tempCatInfo.CatalogFileType;
                FJ.FilePath = tempCatInfo.CatalogFilePath;
                FJ.RecordLength = tempCatInfo.CatalogRecordLength;
                FJ.VsamSegmentName = tempCatInfo.VsamSegmentName;
                FJ.VsamKeys = tempCatInfo.VsamKeys;
                FJ.FileStatus = FileStatus.Successful_completion;
                FJ.ReadPointer = 0;

            }

            return FJ;

        }

        public void Remove(IFileLink fileLink)
        {
            if (Files.Values.Contains(fileLink))
            {
                KeyValuePair<string, Common.IFileLink> keyValuePair = Files.Where(kv => kv.Value.Equals(fileLink)).SingleOrDefault();
                string key = keyValuePair.Key;
                Remove(key);
            }
        }

        public void Remove(string name)
        {
            Files.Remove(name);
        }

        public void SendError(string message)
        {
            message = String.Format("{0}: Error - {1}", LOG_PREFIX, message);
            Console.WriteLine(message);
        }

        public void SendDebugMessage(string message)
        {
            if (this.IsDebugging)
            {
                Console.WriteLine(String.Format("{0}: Debug - {1}", LOG_PREFIX, message));
            }
        }

        public void SendWarning(string message)
        {
            message = String.Format("{0}: Warning - {1}", LOG_PREFIX, message);
            Console.WriteLine(message);
        }

        public void SendMessage(string message)
        {
            Console.WriteLine(String.Format("{0}: {1}", LOG_PREFIX, message));
        }

        public ICatalogFileInfo GetCatalogFileInfo(string fileName)
        {
            if (fileName == null)
                return null;

            ICatalogFileInfo catFileInfo = new CatalogFileInfo();

            if (isCatalogInDB)
            {
                //DataTable dtCatFileInfo= new DataTable();
                //dtCatFileInfo = jesDBAccess.ExecuteSelectStoredProc("usp_Catalog_File_SelectByName", new List<SqlParameter> { new SqlParameter("@CatalogFileName", fileName) });
                //if (dtCatFileInfo.Rows.Count == 0)
                //    return null;
                //catFileInfo.CatalogFileID = (int)dtCatFileInfo.Rows[0]["Catalog_File_ID"];
                //catFileInfo.CatalogFilePath = (string)dtCatFileInfo.Rows[0]["Full_Path"];
                //catFileInfo.CatalogRecordLength = (int)dtCatFileInfo.Rows[0]["File_Record_Length"];
                //catFileInfo.IsCataloged = (bool)dtCatFileInfo.Rows[0]["Is_Cataloged"];
                //string fileOrg = (string)dtCatFileInfo.Rows[0]["File_Record_Format"];
                //if (fileOrg == "Fixed")
                //    catFileInfo.CatalogFileOrganization = FileOrganization.Fixed;
                //else if (fileOrg == "LineSequential")
                //    catFileInfo.CatalogFileOrganization = FileOrganization.LineSequential;
                //else if (fileOrg == "Folder")
                //    catFileInfo.CatalogFileOrganization = FileOrganization.UnKnown;
                //else if (fileOrg == "Variable")
                //    catFileInfo.CatalogFileOrganization = FileOrganization.Variable;
                //else if (fileOrg == "FBA")
                //    catFileInfo.CatalogFileOrganization = FileOrganization.FBA;
                //else if (fileOrg == "Indexed")
                //    catFileInfo.CatalogFileOrganization = FileOrganization.Indexed;
                //else catFileInfo.CatalogFileOrganization = FileOrganization.UnKnown;

                //string fileType = (string)dtCatFileInfo.Rows[0]["File_Type"];
                //if (fileType == "1")
                //    catFileInfo.CatalogFileType = FileType.FLAT;
                //else if (fileType == "5")
                //    catFileInfo.CatalogFileType = FileType.GDG;
                //else if (fileType == "6")
                //    catFileInfo.CatalogFileType = FileType.VSAM_SQLServer;
                //else if (fileType == "7")
                //    catFileInfo.CatalogFileType = FileType.VSAM_Oracle;
                //else
                //    catFileInfo.CatalogFileType = FileType.UNKNOWN;

                //if (dtCatFileInfo.Columns.Contains("Segment_Name"))
                //{
                //    catFileInfo.VsamSegmentName = (string)dtCatFileInfo.Rows[0]["Segment_Name"];
                //}
                //else
                //{
                //    catFileInfo.VsamSegmentName = string.Empty;
                //}

                //if (dtCatFileInfo.Columns.Contains("Vsam_Keys"))
                //{
                //    catFileInfo.VsamKeys = (string)dtCatFileInfo.Rows[0]["Vsam_Keys"];
                //}
                //else
                //{
                //    catFileInfo.VsamKeys = string.Empty;
                //}
            }
            else
            {
                if (fileName == "'con'")
                {
                    catFileInfo.CatalogFileOrganization = FileOrganization.LineSequential;
                    catFileInfo.CatalogFileType = FileType.FLAT;
                    catFileInfo.CatalogFilePath = "Console";
                }
                else
                {
                    string fileInfomation = System.Environment.GetEnvironmentVariable(fileName);
                    if (!string.IsNullOrEmpty(fileInfomation))
                    {
                        catFileInfo.CatalogFileOrganization = FileOrganization.LineSequential;
                        catFileInfo.CatalogFileType = FileType.FLAT;
                        string[] fileStrings = fileInfomation.Split(';');
                        int ctr = 0;
                        foreach (string fileString in fileStrings)
                        {
                            if (ctr == 0)
                            { catFileInfo.CatalogFilePath = fileString; }
                            else if (ctr == 1)
                            { catFileInfo.CatalogRecordLength = Convert.ToInt32(fileString); }
                            else if (ctr == 2)
                            {
                                if (fileString == "LSEQ")
                                    catFileInfo.CatalogFileOrganization = FileOrganization.LineSequential;
                                if (fileString == "LSEQC")
                                    catFileInfo.CatalogFileOrganization = FileOrganization.LineSequentialCompressed;
                                if (fileString == "FB")
                                    catFileInfo.CatalogFileOrganization = FileOrganization.Fixed;
                                else if (fileString == "VB")
                                    catFileInfo.CatalogFileOrganization = FileOrganization.Variable;
                                else if (fileString == "FBA")
                                    catFileInfo.CatalogFileOrganization = FileOrganization.FBA;
                                else if (fileString == "IDX")
                                    catFileInfo.CatalogFileOrganization = FileOrganization.Indexed;
                                else
                                    catFileInfo.CatalogFileOrganization = FileOrganization.LineSequential;
                            }
                            else if (ctr == 3)
                            {
                                if (fileString == "GDG")
                                    catFileInfo.CatalogFileType = FileType.GDG;
                                else if (fileString == "VSAMSQL")
                                    catFileInfo.CatalogFileType = FileType.VSAM_SQLServer;
                                else catFileInfo.CatalogFileType = FileType.FLAT;
                            }
                            ctr++;
                        }
                    }
                    else
                    {
                        catFileInfo = null;
                    }
                }
            }

            return catFileInfo;
        }

        public bool CreateCatalogFile(string catalogFileName, int catalogRecordLength, FileOrganization catalogFileOrganization, FileType catalogFileType, string storageUnit)
        {
            if (isCatalogInDB)
            {
                #region Create Database Catalog Entry
                //    DataTable dtStoragePaths = jesDBAccess.ExecuteSelectStoredProc("usp_Storage_Unit_SelectByName", new List<SqlParameter> { new SqlParameter("@Name", storageUnit) });
                //    if (dtStoragePaths.Rows.Count == 0)
                //        dtStoragePaths = jesDBAccess.ExecuteSelectStoredProc("usp_Storage_Unit_SelectByName", new List<SqlParameter> { new SqlParameter("@Name", "Default") });

                //    string storagePath = string.Concat((string)dtStoragePaths.Rows[0]["Path"], @"\", catalogFileName.Replace(".", @"\"));
                //    int returnCode = jesDBAccess.ExecuteInsertStoredProc("usp_Catalog_File_Insert", new List<SqlParameter> {
                //    new SqlParameter("@CatalogFileName", catalogFileName)
                //   ,new SqlParameter("@FullPath", storagePath)
                //   ,new SqlParameter("@FileType", 1)
                //   ,new SqlParameter("@FileRecordFormat", catalogFileOrganization.ToString())
                //   ,new SqlParameter("@FileRecordLength", catalogRecordLength)
                //   ,new SqlParameter("@FileRetention", 0)
                //   ,new SqlParameter("@DateCreated", DateTime.Now)
                //}); 
                #endregion
            }
            else
            {
                string fileInformation = string.Concat(catalogFileName, ":", catalogRecordLength, ":",
                    catalogFileOrganization.ToString(), ":", catalogFileType.ToString());


                System.Environment.SetEnvironmentVariable(catalogFileName, fileInformation, EnvironmentVariableTarget.Process);
            }

            return true;
        }

        public bool CreateVsamCatalogFile(string catalogFileName, string catalogStoragePath, int catalogFileType, FileOrganization catalogFileOrganization, int catalogRecordLength, string catalogVsamKeys, string catalogSegmentName, string storageUnit)
        {
            if (isCatalogInDB)
            {
                #region Create Database Catalog Entry
                //    int returnCode = jesDBAccess.ExecuteInsertStoredProc("usp_Catalog_File_Insert", new List<SqlParameter> {
                //    new SqlParameter("@CatalogFileName", catalogFileName)
                //   ,new SqlParameter("@FullPath", catalogStoragePath)
                //   ,new SqlParameter("@FileType", catalogFileType)
                //   ,new SqlParameter("@FileRecordFormat", catalogFileOrganization.ToString())
                //   ,new SqlParameter("@FileRecordLength", catalogRecordLength)
                //   ,new SqlParameter("@FileRetention", 0)
                //   ,new SqlParameter("@vsam_keys", catalogVsamKeys)
                //   ,new SqlParameter("@segment_name", catalogSegmentName)
                //   ,new SqlParameter("@DateCreated", DateTime.Now)
                //});
                #endregion
            }
            else
            {
                string fileInformation = string.Concat(catalogFileName, ":", catalogRecordLength, ":",
                      catalogFileOrganization.ToString(), ":", catalogFileType.ToString());


                System.Environment.SetEnvironmentVariable(catalogFileName, fileInformation, EnvironmentVariableTarget.Process);
            }

            return true;
        }

        public bool DeleteCatalogFile(string catalogFileName)
        {
            ICatalogFileInfo catInfo = GetCatalogFileInfo(catalogFileName);
            if (catInfo != null)
            {
                if (isCatalogInDB)
                {
                    #region Delete Database Catalog Entry
                    //if (catInfo.CatalogFileType == FileType.VSAM_SQLServer)
                    //{
                    //    if (catInfo.VsamSegmentName.Trim() == string.Empty)
                    //    {
                    //        jesDBAccess.ExecuteSelectStoredProc("usp_Catalog_File_DeleteByName",
                    //        new List<SqlParameter> { new SqlParameter("@CatalogFileName", catalogFileName) });
                    //    }
                    //    else
                    //    {
                    //        jesDBAccess.ExecuteSelectStoredProc("usp_Catalog_File_Update", new List<SqlParameter> {
                    //        new SqlParameter("@CatalogFileID", catInfo.CatalogFileID)
                    //       ,new SqlParameter("@IsCataloged", 0)
                    //    });
                    //    }
                    //}
                    //else
                    //{
                    //    jesDBAccess.ExecuteSelectStoredProc("usp_Catalog_File_DeleteByName",
                    //        new List<SqlParameter> { new SqlParameter("@CatalogFileName", catalogFileName) });
                    //}
                    #endregion
                }
                else
                {
                    //Delete Enviroenemt Variable
                    System.Environment.SetEnvironmentVariable(catalogFileName, string.Empty,EnvironmentVariableTarget.Process);
                }
            }

            return true;
        }

        public bool UpdateVsamCatalogFile(string catalogFileName)
        {
            ICatalogFileInfo catInfo = GetCatalogFileInfo(catalogFileName);
            if (catInfo != null)
            {
                if (isCatalogInDB)
                {
                    #region Update Database Catalog Entry
                    //    jesDBAccess.ExecuteSelectStoredProc("usp_Catalog_File_Update", new List<SqlParameter> {
                    //    new SqlParameter("@CatalogFileID", catInfo.CatalogFileID)
                    //    ,new SqlParameter("@IsCataloged", 1)
                    //});
                    #endregion
                }
                else
                {
                    //Do anything here??
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        #endregion

        #region private methods

        private void Initialize()
        {

            IsDebugging = false;

            Int32 jobId;
            FileOrganization format;

            //get environment variable for job id
            try
            {
                if (isCatalogInDB)
                {
                    jobIdValue = System.Environment.GetEnvironmentVariable(envVariableName);
                    if (jobIdValue == null)
                    {
                        SendError(string.Format("Environment variable {0} is null.  Process is terminating!", envVariableName));
                        throw new Exception(string.Format("The requested environment value {0} cannot be found.  Process cannot continue", envVariableName));
                    }
                    if (!Int32.TryParse(jobIdValue, out jobId))
                    {
                        SendError(string.Format("JobID Environment variable {0} is not numeric.  Process is terminating!", envVariableName));
                        throw new Exception(string.Format("The JobID environment value is not numeric.  Process cannot continue!", envVariableName));
                    }
                }
            }
            catch (Exception ex)
            {
                SendError(string.Format("Process is unable to retrieve requested environment variable value {0} due to error {1}.  Process is terminating!", envVariableName, ex.Message));
                throw new Exception("Either the requested environment value is null or the user does not have the required permission to retrieve it.  Process cannot continue");
            }


            //get environment variable for runtime connection string
            try
            {
                if (isCatalogInDB)
                    batchRuntimeConnectionString = System.Environment.GetEnvironmentVariable(envVariableConnectionString);
            }
            catch (Exception ex)
            {
                SendError(string.Format("Process is unable to retrieve requested environment variable value {0} due to error {1}.  Process is terminating!", envVariableConnectionString, ex.Message));
                throw new Exception("Either the requested environment value is null or the user does not have the required permission to retrieve it.  Process cannot continue");
            }

            SendDebugMessage("Retrieving runtime job step get files for runtime job step id " + jobIdValue);

            try
            {
                if (isCatalogInDB)
                {
                    #region Get Files Database Files collections
                    ////retrieve file meta data from data base for jobId
                    //jesDBAccess = new JESDBAccess(batchRuntimeConnectionString);
                    //DataTable fileDefs = jesDBAccess.ExecuteSelectStoredProc("usp_Runtime_Job_Step_GetFiles", new List<SqlParameter> { new SqlParameter("@RuntimeJobStepID", jobIdValue) });

                    //SendDebugMessage(string.Format("Begin file link creation for {0} files defined in return from stored proc", fileDefs.Rows.Count));
                    ////create a file link for each row in the returned data set
                    //foreach (DataRow drow in fileDefs.Rows)
                    //{
                    //    if (Files.ContainsKey(drow["File_DD"].ToString().Trim()))
                    //    {
                    //        IFileLink test;
                    //        if (Files.TryGetValue(drow["File_DD"].ToString().Trim(), out test))
                    //        {
                    //            if (test.DDName == "SYSOUT")
                    //                continue;
                    //            if (test.DDName != drow["File_DD"].ToString().Trim() ||
                    //                test.FileAccess != (FileAccessMode)Enum.Parse(typeof(FileAccessMode), drow["AccessMode"].ToString().Trim(), true) ||
                    //                test.FileOrganization != ((FileOrganization)Enum.Parse(typeof(FileOrganization), drow["FileOrganization"].ToString().Trim(), true)) ||
                    //                test.RecordLength != (int)drow["RecordLength"])
                    //                throw new Exception("Concatenated files must have the same record definition for each component of the file.  Verify the DDNAME, access, organization, and record length for file DDNAME:" + test.DDName);
                    //            else
                    //                ((FileLink_RemoteBatch)test).FilePaths.Add(drow["File_Path"].ToString());
                    //        }
                    //    }

                    //    FileLink_RemoteBatch fl = new FileLink_RemoteBatch(this, drow["File_DD"].ToString().Trim());

                    //    fl.DDName = drow["File_dd"].ToString().Trim();
                    //    fl.FileAccess = (FileAccessMode)Enum.Parse(typeof(FileAccessMode), drow["AccessMode"].ToString().Trim(), true);
                    //    fl.FileOrganization = Enum.TryParse(drow["FileOrganization"].ToString().Trim().ToString().Trim(), true, out format) == false ? (FileOrganization)Enum.Parse(typeof(FileOrganization), "LineSequential", true) : format;
                    //    fl.RecordLength = (int)drow["RecordLength"];
                    //    if (fl.RecordLength == 0)
                    //        fl.RecordLength = 133;
                    //    fl.FileStatus = FileStatus.Successful_completion;

                    //    fl.FilePath = drow["File_Path"].ToString();

                    //    if (fileDefs.Columns.Contains("File_Name"))
                    //    {
                    //        fl.CatalogFileName = (string)drow["File_Name"];
                    //    }

                    //    this.Add(fl);
                    //}
                    #endregion
                }
                else
                {
                    //throw new NotImplementedException("Get Environment variable files not implemented");
                }
                SendDebugMessage("File handler created successfully");
            }
            catch (Exception e)
            {
                SendError(string.Format("Retrieving file meta data from database for runtime job step id {0} failed due to error '{1}'. Process is terminating!", jobIdValue, e.Message));
                throw new Exception(string.Format("Retrieving file meta data from database for runtime job step id {0} failed due to error '{1}'. Process is terminating!", jobIdValue, e.Message));
            }
        }


        #endregion


        public Common.IFileLink LoadFile(string DDName, string ddValue, FileType FileType)
        {
            if (this.Files.ContainsKey(DDName))
            {
                return this.Files[DDName];
            }
            IFileLink fileLink = null;
            fileLink = new FileLink_RemoteBatch(this, DDName);
            ICatalogFileInfo tempCatInfo = GetCatalogFileInfo(ddValue);
            if (tempCatInfo != null)
            {
                fileLink.FileOrganization = tempCatInfo.CatalogFileOrganization;
                fileLink.FileType = tempCatInfo.CatalogFileType;
                fileLink.FilePath = tempCatInfo.CatalogFilePath;
                fileLink.RecordLength = tempCatInfo.CatalogRecordLength;
                this.Files.Add(fileLink.DDName, fileLink);
            }
            return fileLink;
        }

        public Common.IFileLink LoadFile(string name)
        {
            return LoadFile(name, name, FileType.UNKNOWN);
        }


    }
}
