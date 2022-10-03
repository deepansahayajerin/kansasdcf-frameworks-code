
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSY.Framework.Buffer.Interfaces;

namespace MDSY.Framework.Interfaces
{
    public interface IMapServices
    {
        void SendMap(IMapDefinition mapDefinition, params SendOption[] sendOptions);
        void SendMap(IField mapName, params SendOption[] sendOptions);
        void SendMap(string mapName, params SendOption[] sendOptions);
        void SendMap(IMapDefinition mapDefinition, IBufferValue mapRecordSource, params SendOption[] sendOptions);
        void SendMap(IMapDefinition mapDefinition, IBufferValue mapRecordSource, int cursr, params SendOption[] sendOptions);
        void SendMap(IGroup mapDefinition, IBufferValue mapRecordSource, params SendOption[] sendOptions);
        void SendMap(IField mapSet, IField mapName, IBufferValue mapRecordSource, params SendOption[] sendOptions);
        void SendMap(IField mapSet, IField mapName, IBufferValue mapRecordSource, int Cursr, params SendOption[] sendOptions);
        void SendMap(IField mapSet, IField mapName,  params SendOption[] sendOptions);
        void SendMap(string mapSet, string mapName, params SendOption[] sendOptions);
        void SendMap(string mapName, IBufferValue mapRecordSource, params SendOption[] sendOptions);
        void SendMap(IField mapName, IBufferValue mapRecordSource, params SendOption[] sendOptions);
        void SendMap(IMapDefinition mapDefinition, IBufferValue mapRecordSource, IField respCode, params SendOption[] sendOptions);
        void SendFrom(IBufferValue mapRecordSource, int sendLength, params SendOption[] sendOptions);
        void ReceiveMap(IMapDefinition mapDefinition, IBufferValue mapRecordTarget, IField respCode, params ReceiveOption[] receiveOptions);
        void ReceiveMap(IMapDefinition mapDefinition, IBufferValue mapRecordTarget, params ReceiveOption[] receiveOptions);
        void ReceiveMap(IGroup mapDefinition, IBufferValue mapRecordTarget, params ReceiveOption[] receiveOptions);
        void ReceiveMap(IField mapSet, IField mapName, IBufferValue mapRecordSource, params ReceiveOption[] receiveOptions);
        void ReceiveMap(string mapName, IBufferValue mapRecordTarget, params ReceiveOption[] receiveOptions);
        void ReceiveMap(IField mapName, IBufferValue mapRecordTarget, params ReceiveOption[] receiveOptions);
        void ReceiveData(IBufferValue dataRecordTarget, int dataLength, params ReceiveOption[] receiveOptions);
        void SendText( IBufferValue mapRecordSource, params SendOption[] sendOptions);
        void SendText(IBufferValue mapRecordSource, int length, params SendOption[] sendOptions);
    }
}
