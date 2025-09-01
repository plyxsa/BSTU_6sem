module.exports = (sequelize, DataTypes) => {
    const AuditoriumType = sequelize.define('AuditoriumType', {
        AUDITORIUM_TYPE: {
            type: DataTypes.STRING(10),
            allowNull: false,
            primaryKey: true
        },
        AUDITORIUM_TYPENAME: {
            type: DataTypes.STRING(50),
            allowNull: false,
            unique: true
        }
    }, {
        tableName: 'AUDITORIUM_TYPE',
        timestamps: false
    });
    return AuditoriumType;
};