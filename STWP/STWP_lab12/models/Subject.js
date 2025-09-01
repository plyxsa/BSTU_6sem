module.exports = (sequelize, DataTypes) => {
    const Subject = sequelize.define('Subject', {
        SUBJECT: {
            type: DataTypes.STRING(20),
            allowNull: false,
            primaryKey: true
        },
        SUBJECT_NAME: {
            type: DataTypes.STRING(100),
            allowNull: false
        },
        PULPIT: {
            type: DataTypes.STRING(20),
            allowNull: false
        }
    }, {
        tableName: 'SUBJECT',
        timestamps: false
    });
    return Subject;
};