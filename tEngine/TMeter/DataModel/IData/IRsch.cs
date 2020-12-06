using System;
using System.Collections.Generic;
using tEngine.DataModel;

namespace tEngine.TMeter.DataModel.IData {
    public interface IRsch<T> {
        /// <summary>
        /// ����������� �����, ��������
        /// </summary>
        string Comment { get; set; }

        /// <summary>
        /// ����� ��������
        /// </summary>
        DateTime CreateTime { get; set; }

        /// <summary>
        /// ���������� ���� � �����
        /// </summary>
        string FilePath { get; set; }

        /// <summary>
        /// ���������� �������������
        /// </summary>
        Guid ID { get; set; }

        /// <summary>
        /// ������������ ID ���������� ���������
        /// </summary>
        List<Guid> MsmsGuids { get; set; }

        /// <summary>
        /// �������� 
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// ������ ����� �������������
        /// </summary>
        List<string> UsersPaths { get; set; }

        /// <summary>
        /// �������� ����� ���������
        /// </summary>
        /// <param name="user"></param>
        /// <param name="msmId"></param>
        void AddMsm( User user, Guid msmId );

        /// <summary>
        /// �������� ���� ���������
        /// </summary>
        /// <returns></returns>
        Msm GetMsm( Guid msmId );

        /// <summary>
        /// ���������� ���������
        /// </summary>
        /// <returns></returns>
        int GetMsmCount();

        /// <summary>
        /// �������� ����� ���������
        /// </summary>
        /// <returns></returns>
        IEnumerable<Msm> GetMsms();

        /// <summary>
        /// �������� ����� ���������
        /// </summary>
        /// <returns></returns>
        IEnumerable<User> GetUsers();

        /// <summary>
        /// ���������� ���������
        /// </summary>
        /// <returns></returns>
        int GetUsersCount();

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        bool Open( string filePath, out T rsch );

        /// <summary>
        /// ������� ��������� �� ������
        /// </summary>
        /// <param name="msmId"></param>
        void RemoveMsm( Guid msmId );

        /// <summary>
        /// ������� ��������� �� ������
        /// </summary>
        /// <param name="msm"></param>
        void RemoveMsm( Msm msm );

        /// <summary>
        /// ��������� � ���� �� ���������
        /// </summary>
        bool Save();

        /// <summary>
        /// ��������� � ����� ����
        /// </summary>
        /// <param name="filePath"></param>
        bool Save( string filePath );
    }
}