using System;
using System.Collections.Generic;

namespace tEngine.TMeter.DataModel.IData {
    public interface IUser<T> {
        /// <summary>
        /// ���� ��������
        /// </summary>
        DateTime BirthDate { get; set; }

        /// <summary>
        /// ����������� �����
        /// </summary>
        string Comment { get; set; }

        /// <summary>
        /// ���������� ���� � ����� ��������
        /// </summary>
        string FilePath { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        string FName { get; set; }

        /// <summary>
        /// ���������� �������������
        /// </summary>
        Guid ID { get; set; }

        /// <summary>
        /// ������ ����������� ��������� 
        /// </summary>
        List<Msm> Msms { get; set; }

        /// <summary>
        /// ���
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        string SName { get; set; }

        /// <summary>
        /// �������� ���������
        /// </summary>
        /// <param name="msm"></param>
        void AddMsm( Msm msm );

        /// <summary>
        /// �������� ��������� �� ������
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Msm GetMsm( int index );

        /// <summary>
        /// �������� ��������� �� ID
        /// </summary>
        /// <param name="msmId"></param>
        /// <returns></returns>
        Msm GetMsm( Guid msmId );

        /// <summary>
        /// ���������� ���������
        /// </summary>
        /// <returns></returns>
        int GetMsmCount();

        /// <summary>
        /// �������� ��������� ���������
        /// </summary>
        /// <returns></returns>
        IEnumerable<Msm> GetMsms();

        /// <summary>
        /// ������� ���� � �������������
        /// </summary>
        /// <returns></returns>
        bool Open( string filePath, out T user );

        /// <summary>
        /// ������� ���������
        /// </summary>
        /// <param name="msm"></param>
        void RemoveMsm( Msm msm );

        /// <summary>
        /// ������� ���������
        /// </summary>
        /// <param name="msm"></param>
        void RemoveMsm( Guid msmId );

        /// <summary>
        /// ��������� �� ��������
        /// </summary>
        /// <returns>�����</returns>
        bool Save();

        /// <summary>
        /// ��������� �� ������ ����
        /// </summary>
        /// <param name="filePath">���� � �����</param>
        /// <returns>�����</returns>
        bool Save( string filePath );

        /// <summary>
        /// ��������
        /// </summary>
        /// <returns></returns>
        string UserShort();
    }
}