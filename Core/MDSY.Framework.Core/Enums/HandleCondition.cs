﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDSY.Framework.Core
{
    /// <summary>
    /// List of Handle conditions:
    /// DISABLED, NOTOPEN, NOTFOUND, ERROR, NORMAL
    /// </summary>
    public enum HandleCondition
    {
        NORMAL = 0,
        ERROR = 1,
        RDATT = 2,
        WRBRK = 3,
        EOF = 4,
        EODS = 5,
        EOC = 6,
        INBFMH = 7,
        ENDINPT = 8,
        NONVAL = 9,
        NOSTART = 10,
        TERMIDERR = 11,
        FILENOTFOUND = 12,
        NOTFND = 13,
        DUPREC = 14,
        DUPKEY = 15,
        INVREQ = 16,
        IOERR = 17,
        NOSPACE = 18,
        NOTOPEN = 19,
        ENDFILE = 20,
        ILLOGIC = 21,
        LENGERR = 22,
        QZERO = 23,
        SIGNAL = 24,
        QBUSY = 25,
        ITEMERR = 26,
        PGMIDERR = 27,
        TRANSIDERR = 28,
        ENDDATA = 29,
        INVTSREQ = 30,
        EXPIRED = 31,
        RETPAGE = 32,
        RTEFAIL = 33,
        RTESOME = 34,
        TSIOERR = 35,
        MAPFAIL = 36,
        INVERRTERM = 37,
        INVMPSZ = 38,
        IGREQID = 39,
        OVERFLOW = 40,
        INVLDC = 41,
        NOSTG = 42,
        JIDERR = 43,
        QIDERR = 44,
        NOJBUFSP = 45,
        DSSTAT = 46,
        SELNERR = 47,
        FUNCERR = 48,
        UNEXPIN = 49,
        NOPASSBKRD = 50,
        NOPASSBKWR = 51,
        SEGIDERR = 52,
        SYSIDERR = 53,
        ISCINVREQ = 54,
        ENQBUSY = 55,
        ENVDEFERR = 56,
        IGREQCD = 57,
        SESSIONERR = 58,
        SYSBUSY = 59,
        SESSBUSY = 60,
        NOTALLOC = 61,
        CBIDERR = 62,
        INVEXITREQ = 63,
        INVPARTNSET = 64,
        INVPARTN = 65,
        PARTNFAIL = 66,
        USERIDERR = 69,
        NOTAUTH = 70,
        VOLIDERR = 71,
        SUPPRESSED = 72,
        RESIDERR = 75,
        NOSPOOL = 80,
        TERMERR = 81,
        ROLLEDBACK = 82,
        END = 83,
        DISABLED = 84,
        ALLOCERR = 85,
        STRELERR = 86,
        OPENERR = 87,
        SPOLBUSY = 88,
        SPOLERR = 89,
        NODEIDERR = 90,
        TASKIDERR = 91,
        TCIDERR = 92,
        DSNNOTFOUND = 93,
        LOADING = 94,
        MODELIDERR = 95,
        OUTDESCRERR = 96,
        PARTNERIDERR = 97,
        PROFILEIDERR = 98,
        NETNAMEIDERR = 99,
        LOCKED = 100,
        RECORDBUSY = 101,
        UOWNOTFOUND = 102,
        UOWLNOTFOUND = 103,
        LINKABEND = 104,
        CHANGED = 105,
        PROCESSBUSY = 106,
        ACTIVITYBUSY = 107,
        PROCESSERR = 108,
        ACTIVITYERR = 109,
        CONTAINERERR = 110,
        EVENTERR = 111,
        TOKENERR = 112,
        NOTFINISHED = 113,
        POOLERR = 114,
        TIMERERR = 115,
        SYMBOLERR = 116,
        TEMPLATERR = 117,
        NOTSUPERUSER = 118,
        CSDERR = 119,
        DUPRES = 120,
        RESUNAVAIL = 121,
        CHANNELERR = 122,
        CCSIDERR = 123,
        TIMEDOUT = 124,
        CODEPAGEERR = 125,
        INCOMPLETE = 126,
        APPNOTFOUND = 127,
        DSIDERR = 128,
        ABEND = 999
        
    }
}
