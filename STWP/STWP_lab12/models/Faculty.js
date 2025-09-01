module.exports = (sequelize, DataTypes) => {
    const Faculty = sequelize.define('Faculty', {
        FACULTY: {
            type: DataTypes.STRING(10), // STRING для NVARCHAR
            allowNull: false,
            primaryKey: true
        },
        FACULTY_NAME: {
            type: DataTypes.STRING(100),
            allowNull: false,
            unique: true
        }
    }, {
        tableName: 'FACULTY',
        timestamps: false
    });
    return Faculty;
};